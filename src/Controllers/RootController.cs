//path: src\Controllers\RootController.cs

using Microsoft.AspNetCore.Mvc;

using Neurocache.Utilities;

namespace Neurocache.src.Controllers
{
    [ApiController]
    public class RootController : ControllerBase
    {
        [HttpGet("/")]
        public IActionResult Root()
        {
            var message = $"{VesselInfo.ThisVessel}: Acknowledged.";
            Console.WriteLine(message);
            return Ok(message);
        }
    }
}
