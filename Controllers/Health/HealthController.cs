//path: controllers\Health\HealthController.cs

using Microsoft.AspNetCore.Mvc;

namespace neurocache_gateway.Controllers.Health
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        const string message = "Neurocache Gateway is healthy!";

        [HttpGet]
        public IActionResult Get()
        {
            Console.WriteLine(message);
            return Ok(message);
        }
    }
}
