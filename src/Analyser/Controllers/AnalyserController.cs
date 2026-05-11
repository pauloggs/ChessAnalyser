using Analyser.Models;
using Interfaces;
using Interfaces.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Repositories;
using Services;

namespace Analyser.Controllers;

[ApiController]
[Route("[controller]")]
public class AnalyserController(
    IChessRepository chessRepository,
    IEtlService etlService,
    IEtlProgressStore progressStore,
    IServiceScopeFactory scopeFactory,
    IOptions<PgnOptions> pgnOptions) : ControllerBase
{
    private readonly IChessRepository _chessRepository = chessRepository;
    private readonly IEtlService _etlService = etlService;
    private readonly IEtlProgressStore _progressStore = progressStore;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly PgnOptions _pgnOptions = pgnOptions.Value;

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
                DisplayName = FormatPlayerDisplayName(p)
            })
            .ToList();

        return Ok(options);
    }

    /// <summary>
    /// Get one page of games from the database (ordered by Id). Defaults: page 1, page 50; pageSize is capped at 500.
    /// Optional filters combine with AND. Games with null <c>GameYear</c> are excluded when a year bound is set.
    /// </summary>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Rows per page (1–500).</param>
    /// <param name="minGameYear">Lower bound on <c>GameYear</c> (inclusive).</param>
    /// <param name="maxGameYear">Upper bound on <c>GameYear</c> (inclusive).</param>
    /// <param name="whitePlayerId">Exact match on <c>WhitePlayerId</c>.</param>
    /// <param name="blackPlayerId">Exact match on <c>BlackPlayerId</c>.</param>
    /// <param name="eco">Exact match on persisted <c>Eco</c> (trimmed, max 16 characters).</param>
    /// <param name="cancellationToken">Propagates cancellation from the client.</param>
    [HttpGet("GetGames")]
    [Produces("application/json")]
    public async Task<IActionResult> GetGames(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] short? minGameYear = null,
        [FromQuery] short? maxGameYear = null,
        [FromQuery] int? whitePlayerId = null,
        [FromQuery] int? blackPlayerId = null,
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

        GamePageFilters? filters = minGameYear.HasValue || maxGameYear.HasValue || whitePlayerId.HasValue || blackPlayerId.HasValue || ecoTrimmed != null
            ? new GamePageFilters
            {
                MinGameYear = minGameYear,
                MaxGameYear = maxGameYear,
                WhitePlayerId = whitePlayerId,
                BlackPlayerId = blackPlayerId,
                Eco = ecoTrimmed
            }
            : null;

        var pageResult = await _chessRepository.GetGamesPage(page, pageSize, filters, cancellationToken).ConfigureAwait(false);
        return Ok(pageResult);
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
