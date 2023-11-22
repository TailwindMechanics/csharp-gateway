//path: src\Controllers\RootController.cs

using Microsoft.AspNetCore.Mvc;

using Neurocache.ShipsInfo;

namespace Neurocache.src.Controllers
{
    [ApiController]
    public class RootController : ControllerBase
    {
        [HttpGet("/")]
        public IActionResult Root()
        {
            var message = $"{Ships.ThisVessel}: Acknowledged.";
            Console.WriteLine(message);
            return Ok(message);
        }
    }
}
