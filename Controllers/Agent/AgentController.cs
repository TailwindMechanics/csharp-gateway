//path: controllers\Agent\AgentController.cs

using Microsoft.AspNetCore.Mvc;

using Neurocache.Gateway.Controllers.Agent.Schema;
using Neurocache.Gateway.Utilities;

namespace Neurocache.Gateway.Controllers.Agent
{
    [ApiController]
    [Route("[controller]")]
    public partial class AgentController : ControllerBase
    {
        [HttpPost("start")]
        public IActionResult StartAgent([FromBody] StartAgentRequest body)
        {
            if (!Keys.Guard(Request, out var apiKey))
                return Unauthorized();

            body.Deconstruct(out var instanceId, out var prompt);
            return Ok(instanceId.StartMessage(prompt));
        }

        [HttpGet("stream")]
        public IActionResult StreamAgent([FromQuery] StopAgentRequest query)
        {
            if (!Keys.Guard(Request, out var apiKey))
                return Unauthorized();

            query.Deconstruct(out var instanceId);
            query.Deconstruct(out var outputLevel);

            return Ok(instanceId.StreamMessage(outputLevel));
        }

        [HttpPost("stop")]
        public IActionResult StopAgent([FromBody] StopAgentRequest body)
        {
            if (!Keys.Guard(Request, out var apiKey))
                return Unauthorized();

            body.Deconstruct(out var instanceId);
            return Ok(instanceId.StopMessage());
        }
    }
}
