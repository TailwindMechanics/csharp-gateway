//path: controllers\Agent\AgentController.cs

using Microsoft.AspNetCore.Mvc;
using Serilog;

using Neurocache.Gateway.Controllers.Agent.Schema;

namespace Neurocache.Gateway.Controllers.Agent
{
    [ApiController]
    [Route("[controller]")]
    public class AgentController : ControllerBase
    {
        public record Viewport(float X, float Y, float Zoom);
        public record AgentGraphData(Node[] Nodes, Edge[] Edges, Viewport? Viewport);
        public record RunAgentRequest(string AgentId, string Prompt);
        public record StopAgentRequest(string InstanceId);

        [HttpPost("stop")]
        public IActionResult StopAgent([FromBody] StopAgentRequest body)
        {
            if (!Guid.TryParse(Request.Headers["apikey"], out var apiKey))
            {
                Log.Information("Unauthorized: Invalid API Key format");
                return Unauthorized();
            }

            body.Deconstruct(out var instanceId);
            var message = $"<stop [{instanceId}]>";
            Log.Information(message);
            return Ok(message);
        }

        [HttpPost("run")]
        public IActionResult RunAgent([FromBody] RunAgentRequest body)
        {
            if (!Guid.TryParse(Request.Headers["apikey"], out var apiKey))
            {
                Log.Information("Unauthorized: Invalid API Key format");
                return Unauthorized();
            }

            body.Deconstruct(out var agentId, out var prompt);
            return new PushStreamResult(StreamLoop, "text/event-stream");
        }

        async Task StreamLoop(Stream stream, HttpContext httpContext)
        {
            var instanceId = Guid.NewGuid();
            var writer = new StreamWriter(stream);
            await Emit(writer, $"<start [{instanceId}]>");

            for (int i = 4; i >= 0; i--)
            {
                if (httpContext.RequestAborted.IsCancellationRequested)
                    break;

                await Task.Delay(1000);
                await Emit(writer, $"<emit [{instanceId}], [{i}]>");
            }

            await Task.Delay(1000);
            await Emit(writer, $"</end [{instanceId}]>");
        }

        async Task Emit(StreamWriter writer, string emission)
        {
            Log.Information(emission);
            await writer.WriteLineAsync($"data: {emission}");
            await writer.FlushAsync();
        }
    }
}
