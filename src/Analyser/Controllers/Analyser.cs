using Microsoft.AspNetCore.Mvc;

namespace src.Analyser.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Analyser : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok();
        }
    }
}
