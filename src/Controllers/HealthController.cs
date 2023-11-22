//path: src\Controllers\HealthController.cs

using Microsoft.AspNetCore.Mvc;

using Neurocache.ShipsInfo;

namespace Neurocache.src.Controllers
{
    [ApiController]
    public class HealthController : Controller
    {
        [HttpGet("health")]
        public IActionResult Health()
        {
            Ships.Log("All systems normal");
            return Ok();
        }
    }
}
