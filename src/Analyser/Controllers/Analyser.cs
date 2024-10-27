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

        [HttpGet("LoadGames")]
        public IActionResult LoadGames(string filePath = "C:\\PGN")
        {
            etlService.LoadGamesToDatabase(filePath);
            return Ok();
        }

        [HttpGet("GetGames")]
        public async Task<IActionResult> GetGames()
        {
            var games = await chessRepository.GetGames();
            return Ok(games);
        }
    }
}
