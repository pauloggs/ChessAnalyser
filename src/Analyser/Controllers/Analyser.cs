using Microsoft.AspNetCore.Mvc;
using Repositories;
using Services;

namespace Analyser.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Analyser(IFileHandler fileHandler, IChessRepository chessRepository) : ControllerBase
    {
        private readonly IFileHandler fileHandler = fileHandler;
        private readonly IChessRepository chessRepository = chessRepository;

        [HttpGet("LoadGames")]
        public IActionResult LoadGames(string filePath = "C:\\PGN")
        {
            fileHandler.LoadPgnFiles(filePath);
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
