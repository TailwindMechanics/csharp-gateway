//path: controllers\Agent\Internal\OldController.cs

using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Neurocache.Gateway.Controllers.Agent.Internal
{
    public class OldController : ControllerBase
    {
        public record RunAgentRequest(string AgentId, string Prompt);

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
