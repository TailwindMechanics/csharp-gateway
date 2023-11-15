//path: controllers\Agent\AgentController.cs

using Microsoft.AspNetCore.Mvc;
using Supabase;

using Neurocache.Gateway.Utilities;
using Neurocache.Gateway.Schema;

namespace Neurocache.Gateway.Controllers.Agent
{
    [ApiController]
    [Route("[controller]")]
    public class AgentController : ControllerBase
    {
        private readonly Client supabaseClient;
        public AgentController(Client supabaseClient)
            => this.supabaseClient = supabaseClient;

        [HttpPost("start")]
        public async Task<IActionResult> StartAgent([FromBody] StartAgentRequest body)
        {
            if (!Keys.Guard(Request, out var apiKey))
                return Unauthorized();

            body.Deconstruct(out var agentId, out var prompt);
            var graph = await AgentUtils.GetAgentGraph(supabaseClient, agentId, apiKey);

            return Ok(agentId.StartMessage(graph!.Nodes[0].Data.NodeName!));
        }

        [HttpGet("stream")]
        public IActionResult StreamAgent([FromQuery] StreamAgentRequest query)
        {
            if (!Keys.Guard(Request, out var apiKey))
                return Unauthorized();

            query.Deconstruct(out var instanceId, out var outputLevel);
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
