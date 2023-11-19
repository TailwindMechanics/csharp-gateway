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
            var message = $"{VesselInfo.VesselName}: All systems nominal.";
            Log.Information(message);
            return Ok(message);
        }
    }
}
