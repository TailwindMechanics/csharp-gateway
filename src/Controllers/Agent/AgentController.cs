//path: src\Controllers\Agent\AgentController.cs

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using Supabase;
using Serilog;

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

        [HttpPost("kill")]
        public async Task<IActionResult> Kill()
        {
            if (!Keys.Guard(Request, out var apiKey))
                return Unauthorized();

            using var client = new HttpClient();

            Log.Information("Sending kill request to Nexus");
            await client.PostAsync(StringUtils.NexusRoute("csharp", "kill"), null);

            return Ok("Killed");
        }

        [HttpPost("stop")]
        public async Task<IActionResult> StopAgent([FromBody] StopAgentRequest body)
        {
            if (!Keys.Guard(Request, out var apiKey))
                return Unauthorized();

            body.Deconstruct(out var sessionToken);

            using var client = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(
                new { sessionToken }),
                Encoding.UTF8,
                "application/json"
            );

            Log.Information("Sending stop request to Nexus");
            await client.PostAsync(StringUtils.NexusRoute("csharp", "stop"), null);

            return Ok(sessionToken.StopMessage());
        }

        [HttpPost("run")]
        public async Task<IActionResult> RunAgentAsync([FromBody] RunAgentRequest body)
        {
            Log.Information("StartAgent called with body: {Body}", body);

            if (!Keys.Guard(Request, out var apiKey))
            {
                Log.Warning("Unauthorized access attempt in StartAgent");
                return Unauthorized();
            }

            body.Deconstruct(out var agentId, out var prompt);
            Log.Information("Fetching graph for Agent ID: {AgentId}", agentId);
            var graph = await AgentUtils.GetAgentGraph(supabaseClient, agentId, apiKey);

            Log.Information("Starting Sse Loop");
            return new PushStreamResult(StreamLoop, "text/event-stream");
        }

        async Task StreamLoop(Stream stream, HttpContext httpContext)
        {
            var sessionToken = Guid.NewGuid();
            var writer = new StreamWriter(stream);
            await Emit(writer, $"<start [sessionToken: {sessionToken}]>");

            for (int i = 4; i >= 0; i--)
            {
                if (httpContext.RequestAborted.IsCancellationRequested)
                    break;

                await Task.Delay(1000);
                await Emit(writer, $"<emit [sessionToken: {sessionToken}], [{i}]>");
            }

            await Task.Delay(1000);
            await Emit(writer, $"</end [sessionToken: {sessionToken}]>");
        }

        async Task Emit(StreamWriter writer, string emission)
        {
            Log.Information(emission);
            await writer.WriteLineAsync($"data: {emission}");
            await writer.FlushAsync();
        }
    }
}
