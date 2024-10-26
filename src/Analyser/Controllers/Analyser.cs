using Microsoft.AspNetCore.Mvc;
using Services;

namespace Analyser.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Analyser(IFileHandler fileHandler) : ControllerBase
    {
        private readonly IFileHandler fileHandler = fileHandler;

        [HttpGet]
        public IActionResult LoadGames(string filePath = "C:\\PGN")
        {
            fileHandler.LoadPgnFiles(filePath);
            return Ok();
        }
    }
}
