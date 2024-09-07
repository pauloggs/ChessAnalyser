using Microsoft.AspNetCore.Mvc;

namespace Analyser.Controllers
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
