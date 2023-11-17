//path: src\Controllers\Health\HealthController.cs

using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Neurocache.Gateway.Controllers.Health
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var message = "Csharp Gateway is healthy!";
            Log.Information(message);
            return Ok(message);
        }
    }
}
