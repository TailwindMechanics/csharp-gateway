//path: src\Controllers\HealthController.cs

using Microsoft.AspNetCore.Mvc;

namespace Neurocache.src.Controllers
{
    [ApiController]
    public class HealthController : Controller
    {
        [HttpGet("health")]
        public IActionResult Health()
            => Ok();
    }
}
