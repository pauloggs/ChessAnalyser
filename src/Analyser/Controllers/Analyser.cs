using Microsoft.AspNetCore.Mvc;
using Repositories;
using Services;

namespace Analyser.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Analyser(IChessRepository chessRepository, IEtlService etlService) : ControllerBase
    {
        private readonly IChessRepository chessRepository = chessRepository;
        private readonly IEtlService etlService = etlService;

        /// <summary>
        /// Load PGN files and persist to database.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        [HttpGet("LoadGames")]
        public IActionResult LoadGames(string filePath = "C:\\PGN")
        {
            etlService.LoadGamesToDatabase(filePath);
            return Ok();
        }

        /// <summary>
        /// Get games from database.
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetGames")]
        public async Task<IActionResult> GetGames()
        {
            var games = await chessRepository.GetGames();
            return Ok(games);
        }
    }
}
