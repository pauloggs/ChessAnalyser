using Interfaces;
using Interfaces.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Repositories;
using Services;

namespace Analyser.Controllers;

[ApiController]
[Route("[controller]")]
public class Analyser(
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
    /// Get games from database.
    /// </summary>
    [HttpGet("GetGames")]
    public async Task<IActionResult> GetGames()
    {
        var games = await _chessRepository.GetGames();
        return Ok(games);
    }
}