//path: src\Controllers\Root\RootController.cs

using Microsoft.AspNetCore.Mvc;

namespace Neurocache.Gateway.Controllers.Root
{
    [ApiController]
    [Route("/")]
    public class RootController : ControllerBase
    {
        const string message = "♫ Hello world, this is me ♫";

        [HttpGet]
        public IActionResult Get()
        {
            Console.WriteLine(message);
            return Ok(message);
        }
    }
}
