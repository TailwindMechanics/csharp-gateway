//path: controllers\Agent\AgentController.cs

using Microsoft.AspNetCore.Mvc;

using neurocache_gateway.Controllers.Agent.Schema;

namespace neurocache_gateway.Controllers.Agent
{
    [ApiController]
    [Route("[controller]")]
    public class AgentController : ControllerBase
    {
        public record Viewport(float X, float Y, float Zoom);
        public record AgentGraphData(Node[] Nodes, Edge[] Edges, Viewport? Viewport);
        public record RunAgentRequest(string AgentId, string Prompt);
        public record StopAgentRequest(string InstanceId);

        const string startKey = "<start>";
        const string stopKey = "<stop>";
        const string endKey = "</end>";

        [HttpPost("stop")]
        public IActionResult StopAgent([FromHeader(Name = "apikey")] Guid apiKey,
                    [FromBody] StopAgentRequest request)
        {
            var message = $"Stopped agent {request.InstanceId}";
            Console.WriteLine(message);
            return Ok(message);
        }

        [HttpPost("run")]
        public IActionResult RunAgent(
                [FromHeader(Name = "apikey")] Guid apiKey,
                [FromBody] RunAgentRequest request)
        {
            return new PushStreamResult(async (stream, httpContext) =>
            {
                var message = $"Running agent {request.AgentId}";
                Console.WriteLine(message);

                var writer = new StreamWriter(stream);
                Console.WriteLine(startKey);
                await writer.WriteLineAsync($"data: {startKey}");
                await writer.FlushAsync();

                for (int i = 1; i <= 3; i++)
                {
                    if (httpContext.RequestAborted.IsCancellationRequested)
                        break;

                    await Task.Delay(1000);

                    message = $"data: {i}";
                    Console.WriteLine(message);
                    await writer.WriteLineAsync(message);
                    await writer.FlushAsync();
                }

                await Task.Delay(1000);

                Console.WriteLine(endKey);
                await writer.WriteLineAsync($"data: {endKey}");
                await writer.FlushAsync();
            }, "text/event-stream");
        }
    }
}
