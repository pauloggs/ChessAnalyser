using Analyser.Models;
using Interfaces;
using Interfaces.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Repositories;
using Services;
using Services.FideCatalog;
using Services.Helpers;
using Services.PlayerMetadata;

namespace Analyser.Controllers;

[ApiController]
[Route("[controller]")]
public class AnalyserController(
    IChessRepository chessRepository,
    IEtlService etlService,
    IEtlProgressStore progressStore,
    IMaintenanceProgressStore maintenanceProgressStore,
    IServiceScopeFactory scopeFactory,
    IOptions<PgnOptions> pgnOptions,
    IOptions<FideCatalogSeedOptions> fideCatalogOptions) : ControllerBase
{
    private readonly IChessRepository _chessRepository = chessRepository;
    private readonly IEtlService _etlService = etlService;
    private readonly IEtlProgressStore _progressStore = progressStore;
    private readonly IMaintenanceProgressStore _maintenanceProgressStore = maintenanceProgressStore;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly PgnOptions _pgnOptions = pgnOptions.Value;
    private readonly FideCatalogSeedOptions _fideCatalogOptions = fideCatalogOptions.Value;

    /// <summary>
    /// Get default PGN file path from configuration (for UI default value).
    /// </summary>
    [HttpGet("DefaultPgnPath")]
    [Produces("application/json")]
    public IActionResult GetDefaultPgnPath()
    {
        return Ok(new { defaultFilePath = _pgnOptions.DefaultFilePath });
    }

    /// <summary>
    /// Start loading PGN files and persisting to the database. Returns immediately (202 Accepted).
    /// Poll GET /Analyser/LoadGamesProgress for progress.
    /// If filePath is null or empty, uses the configured default (Pgn:DefaultFilePath).
    /// </summary>
    [HttpPost("LoadGames")]
    public IActionResult LoadGames([FromBody] LoadGamesDto loadGamesDto)
    {
        try
        {
            Console.WriteLine("Controller > LoadGames (starting in background)");
            Constants.DisplayBoardPositions = loadGamesDto.DisplayBoardPosition;
            var filePath = string.IsNullOrWhiteSpace(loadGamesDto?.FilePath)
                ? _pgnOptions.DefaultFilePath
                : loadGamesDto.FilePath.Trim();
            _progressStore.ClearCancel();
            _progressStore.Set(null); // Clear previous run's progress so client doesn't see stale "Completed" and stop polling

            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var etl = scope.ServiceProvider.GetRequiredService<IEtlService>();
                    var progress = new Progress<EtlProgress>(p => _progressStore.Set(p));
                    await etl.LoadGamesToDatabase(filePath, progress);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    _progressStore.Set(new EtlProgress { Status = "Failed", Message = ex.Message });
                }
            });

            return Accepted(new { message = "ETL started. Poll GET /Analyser/LoadGamesProgress for progress." });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Request cancellation of the current ETL. Takes effect before the next game is processed.
    /// </summary>
    [HttpPost("CancelLoad")]
    public IActionResult CancelLoad()
    {
        _progressStore.RequestCancel();
        return Ok(new { message = "Cancel requested." });
    }

    /// <summary>
    /// Get current ETL progress (for progress bar). Returns null when no load has been run or after completion is consumed.
    /// </summary>
    [HttpGet("LoadGamesProgress")]
    [Produces("application/json")]
    public IActionResult GetLoadGamesProgress()
    {
        var progress = _progressStore.Get();
        if (progress == null)
            return Content("null", "application/json");
        return Ok(progress);
    }

    /// <summary>
    /// Gets players for local UI filter dropdowns, ordered by surname and forenames.
    /// </summary>
    [HttpGet("GetPlayers")]
    [Produces("application/json")]
    public async Task<IActionResult> GetPlayers()
    {
        var players = await _chessRepository.GetPlayers().ConfigureAwait(false);
        var options = players
            .OrderBy(p => p.Surname)
            .ThenBy(p => p.Forenames)
            .Select(p => new PlayerOptionResponse
            {
                Id = p.Id,
                Surname = p.Surname?.Trim() ?? string.Empty,
                Forenames = p.Forenames?.Trim() ?? string.Empty,
                DisplayName = FormatPlayerDisplayName(p)
            })
            .ToList();

        return Ok(options);
    }

    /// <summary>FIDE catalog row count and configured list file path.</summary>
    [HttpGet("FideCatalogStatus")]
    [Produces("application/json")]
    public async Task<IActionResult> GetFideCatalogStatus(CancellationToken cancellationToken = default)
    {
        var count = await _chessRepository.GetFideCatalogCountAsync(cancellationToken).ConfigureAwait(false);
        var resolvedPath = RepoRootLocator.ResolveRelativePath(_fideCatalogOptions.ListPath);
        return Ok(new FideCatalogStatusResponse
        {
            CatalogCount = count,
            ConfiguredListPath = _fideCatalogOptions.ListPath,
            ListFileExists = System.IO.File.Exists(resolvedPath)
        });
    }

    /// <summary>
    /// Start loading <c>Ref.FidePlayer</c> from the FIDE TXT file. Returns 202; poll <see cref="GetMaintenanceProgress"/>.
    /// </summary>
    [HttpPost("SeedFideCatalog")]
    public IActionResult SeedFideCatalog([FromBody] SeedFideCatalogRequest? request)
    {
        var cancellationToken = _maintenanceProgressStore.BeginOperation();
        _maintenanceProgressStore.Set(new MaintenanceProgress
        {
            Operation = "SeedFideCatalog",
            Status = "Running",
            Message = "Starting FIDE catalog seed…"
        });

        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var seed = scope.ServiceProvider.GetRequiredService<IFideCatalogSeedService>();
                var progress = new Progress<MaintenanceProgress>(p => _maintenanceProgressStore.Set(p));
                var outcome = await seed.SeedAsync(request?.ListPath, request?.Force ?? false, progress, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    _maintenanceProgressStore.Set(new MaintenanceProgress
                    {
                        Operation = "SeedFideCatalog",
                        Status = "Cancelled",
                        Message = "FIDE catalog seed cancelled by user."
                    });
                    return;
                }

                if (outcome.Skipped)
                {
                    _maintenanceProgressStore.Set(new MaintenanceProgress
                    {
                        Operation = "SeedFideCatalog",
                        Status = "Skipped",
                        Message = outcome.SkipReason
                    });
                    return;
                }

                _maintenanceProgressStore.Set(new MaintenanceProgress
                {
                    Operation = "SeedFideCatalog",
                    Status = "Completed",
                    PercentComplete = 100,
                    RowsProcessed = outcome.RowsInserted,
                    Message = $"Inserted {outcome.RowsInserted:N0} rows ({outcome.RowsSkipped:N0} lines skipped)."
                });
            }
            catch (OperationCanceledException)
            {
                _maintenanceProgressStore.Set(new MaintenanceProgress
                {
                    Operation = "SeedFideCatalog",
                    Status = "Cancelled",
                    Message = "FIDE catalog seed cancelled by user."
                });
            }
            catch (Exception ex)
            {
                _maintenanceProgressStore.Set(new MaintenanceProgress
                {
                    Operation = "SeedFideCatalog",
                    Status = "Failed",
                    Message = ex.Message
                });
            }
            finally
            {
                _maintenanceProgressStore.ClearOperation();
            }
        });

        return Accepted(new { message = "FIDE catalog seed started. Poll GET /Analyser/MaintenanceProgress." });
    }

    /// <summary>
    /// Link <c>App.Player</c> rows to <c>Ref.WorldChampion</c> / <c>Ref.FidePlayer</c>. Returns 202; poll progress.
    /// </summary>
    [HttpPost("LinkPlayerMetadata")]
    public IActionResult LinkPlayerMetadata()
    {
        var cancellationToken = _maintenanceProgressStore.BeginOperation();
        _maintenanceProgressStore.Set(new MaintenanceProgress
        {
            Operation = "LinkPlayerMetadata",
            Status = "Running",
            Message = "Linking player metadata…"
        });

        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var linking = scope.ServiceProvider.GetRequiredService<IPlayerMetadataLinkingService>();
                var progress = new Progress<MaintenanceProgress>(p => _maintenanceProgressStore.Set(p));
                var outcome = await linking.LinkAllPlayersAsync(progress, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    _maintenanceProgressStore.Set(new MaintenanceProgress
                    {
                        Operation = "LinkPlayerMetadata",
                        Status = "Cancelled",
                        Message = "Player metadata link cancelled by user."
                    });
                    return;
                }

                _maintenanceProgressStore.Set(new MaintenanceProgress
                {
                    Operation = "LinkPlayerMetadata",
                    Status = "Completed",
                    PercentComplete = 100,
                    RowsProcessed = outcome.PlayersProcessed,
                    Message =
                        $"processed={outcome.PlayersProcessed}, worldChampion={outcome.WorldChampionLinked}, " +
                        $"fideLinked={outcome.FideLinked}, fideCleared={outcome.FideCleared}, unchanged={outcome.Unchanged}."
                });
            }
            catch (OperationCanceledException)
            {
                _maintenanceProgressStore.Set(new MaintenanceProgress
                {
                    Operation = "LinkPlayerMetadata",
                    Status = "Cancelled",
                    Message = "Player metadata link cancelled by user."
                });
            }
            catch (Exception ex)
            {
                _maintenanceProgressStore.Set(new MaintenanceProgress
                {
                    Operation = "LinkPlayerMetadata",
                    Status = "Failed",
                    Message = ex.Message
                });
            }
            finally
            {
                _maintenanceProgressStore.ClearOperation();
            }
        });

        return Accepted(new { message = "Player metadata link started. Poll GET /Analyser/MaintenanceProgress." });
    }

    /// <summary>Request cancellation of the running maintenance job (FIDE seed or metadata link).</summary>
    [HttpPost("CancelMaintenance")]
    public IActionResult CancelMaintenance()
    {
        _maintenanceProgressStore.RequestCancel();
        return Ok(new { message = "Cancel requested." });
    }

    /// <summary>Progress for FIDE seed or metadata link jobs.</summary>
    [HttpGet("MaintenanceProgress")]
    [Produces("application/json")]
    public IActionResult GetMaintenanceProgress()
    {
        var progress = _maintenanceProgressStore.Get();
        if (progress == null)
            return Content("null", "application/json");
        return Ok(progress);
    }

    /// <summary>
    /// Get one page of games from the database (ordered by Id). Defaults: page 1, page 50; pageSize is capped at 500.
    /// Optional filters combine with AND. Games with null <c>GameYear</c> are excluded when a year bound is set.
    /// </summary>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Rows per page (1–500).</param>
    /// <param name="minGameYear">Lower bound on <c>GameYear</c> (inclusive).</param>
    /// <param name="maxGameYear">Upper bound on <c>GameYear</c> (inclusive).</param>
    /// <param name="whitePlayerSurname">Exact match on White player's surname.</param>
    /// <param name="whitePlayerForenames">Exact match on White player's forenames. Empty string matches blank forenames.</param>
    /// <param name="blackPlayerSurname">Exact match on Black player's surname.</param>
    /// <param name="blackPlayerForenames">Exact match on Black player's forenames. Empty string matches blank forenames.</param>
    /// <param name="eco">Exact match on persisted <c>Eco</c> (trimmed, max 16 characters).</param>
    /// <param name="cancellationToken">Propagates cancellation from the client.</param>
    [HttpGet("GetGames")]
    [Produces("application/json")]
    public async Task<IActionResult> GetGames(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] short? minGameYear = null,
        [FromQuery] short? maxGameYear = null,
        [FromQuery] string? whitePlayerSurname = null,
        [FromQuery] string? whitePlayerForenames = null,
        [FromQuery] string? blackPlayerSurname = null,
        [FromQuery] string? blackPlayerForenames = null,
        [FromQuery] string? eco = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            return BadRequest("page must be >= 1.");
        if (pageSize < 1 || pageSize > 500)
            return BadRequest("pageSize must be between 1 and 500.");
        if (minGameYear.HasValue && maxGameYear.HasValue && minGameYear.Value > maxGameYear.Value)
            return BadRequest("minGameYear must be <= maxGameYear when both are set.");

        var ecoTrimmed = string.IsNullOrWhiteSpace(eco) ? null : eco.Trim();
        if (ecoTrimmed != null && ecoTrimmed.Length > 16)
            return BadRequest("eco must be at most 16 characters after trimming.");

        var whiteSurname = NormalizeOptional(whitePlayerSurname);
        var whiteForenames = NormalizeOptionalAllowEmpty(whitePlayerForenames);
        var blackSurname = NormalizeOptional(blackPlayerSurname);
        var blackForenames = NormalizeOptionalAllowEmpty(blackPlayerForenames);

        GamePageFilters? filters = minGameYear.HasValue
                                    || maxGameYear.HasValue
                                    || whiteSurname != null
                                    || blackSurname != null
                                    || ecoTrimmed != null
            ? new GamePageFilters
            {
                MinGameYear = minGameYear,
                MaxGameYear = maxGameYear,
                WhitePlayerSurname = whiteSurname,
                WhitePlayerForenames = whiteSurname == null ? null : whiteForenames,
                BlackPlayerSurname = blackSurname,
                BlackPlayerForenames = blackSurname == null ? null : blackForenames,
                Eco = ecoTrimmed
            }
            : null;

        var pageResult = await _chessRepository.GetGamesPage(page, pageSize, filters, cancellationToken).ConfigureAwait(false);
        return Ok(pageResult);
    }

    private static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        return value.Trim();
    }

    private static string? NormalizeOptionalAllowEmpty(string? value)
    {
        if (value == null)
            return null;
        return value.Trim();
    }

    private static string FormatPlayerDisplayName(Player player)
    {
        var surname = player.Surname?.Trim() ?? string.Empty;
        var forenames = player.Forenames?.Trim() ?? string.Empty;

        if (surname.Length == 0)
            return forenames.Length == 0 ? $"Player {player.Id}" : forenames;
        if (forenames.Length == 0)
            return surname;

        return $"{surname}, {forenames}";
    }
}
