//path: src\Controllers\HealthController.cs

using Microsoft.AspNetCore.Mvc;
using Serilog;

using Neurocache.Utilities;

namespace Neurocache.src.Controllers
{
    [ApiController]
    public class HealthController : Controller
    {
        [HttpGet("health")]
        public IActionResult Health()
        {
            var message = $"{VesselInfo.ThisVessel}: All systems normal.";
            Log.Information(message);
            return Ok(message);
        }
    }
}
