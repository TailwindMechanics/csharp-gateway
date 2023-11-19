//path: src\Controllers\Agent\AgentController.cs

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using Supabase;
using Serilog;

using Neurocache.Utilities;
using Neurocache.Schema;

namespace Neurocache.Controllers.Agent
{
    [ApiController]
    public class AgentController : ControllerBase
    {
        private readonly Client supabaseClient;

        public AgentController(Client supabaseClient)
            => this.supabaseClient = supabaseClient;

        [HttpGet("/")]
        public IActionResult Root()
        {
            var message = "♫ Hello world, this is me ♫";
            Console.WriteLine(message);
            return Ok(message);
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            var message = "Csharp Gateway is healthy!";
            Log.Information(message);
            return Ok(message);
        }

        [HttpPost("agent/kill")]
        public async Task<IActionResult> Kill()
        {
            if (!Keys.Guard(Request, out var apiKey))
                return Unauthorized();

            using var client = new HttpClient();

            var nexusUrl = Environment.GetEnvironmentVariable("CSHARP_NEXUS_URL")
                + "/kill"!;
            Log.Information($"Sending kill request to {nexusUrl}");
            await client.PostAsync(nexusUrl, null);

            return Ok("Killed");
        }

        [HttpPost("agent/stop")]
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

            var nexusUrl = Environment.GetEnvironmentVariable("CSHARP_NEXUS_URL")
                + "/stop"!;
            Log.Information($"Sending stop request to {nexusUrl}");

            try
            {
                var response = await client.PostAsync(nexusUrl, content);
                Log.Information($"Response from Nexus: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Log.Error("Error sending stop request to Nexus: {@ex}", ex);
                return StatusCode(500, "Error contacting Nexus service.");
            }

            return Ok(sessionToken.StopMessage());
        }

        [HttpPost("agent/run")]
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
