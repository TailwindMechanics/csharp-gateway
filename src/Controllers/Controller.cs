//path: src\Controllers\Controller.cs

using Microsoft.AspNetCore.Mvc;
using Serilog;

using Neurocache.Utilities;
using Neurocache.Schema;

namespace Neurocache.Controllers.Agent
{
    [ApiController]
    public class Controller : ControllerBase
    {
        [HttpPost("agent/kill")]
        public async Task<IActionResult> Kill()
        {
            if (!Keys.Guard(Request)) return Unauthorized();

            await Task.Delay(1000);
            return Ok("Killed");
        }

        [HttpPost("agent/stop")]
        public async Task<IActionResult> StopAgent([FromBody] StopAgentRequest body)
        {
            if (!Keys.Guard(Request)) return Unauthorized();

            await Task.Delay(1000);
            return Ok();
        }

        [HttpPost("agent/run")]
        public async Task<IActionResult> RunAgentAsync([FromBody] OperationRequestData operationRequest)
        {
            if (!Keys.Guard(Request, out var apiKey)) return Unauthorized();

            var operationToken = await Vanguard.Notice.OperationRequest(apiKey, operationRequest);
            if (operationToken is null) return Unauthorized();

            return new OperationChannel(
                async (stream, httpContext) =>
                    await ClientOperationChannel(
                        operationToken,
                        stream,
                        httpContext
                    ), "text/event-stream"
            );
        }

        public async Task ClientOperationChannel(OperationToken operationToken, Stream stream, HttpContext httpContext)
        {
            var writer = new StreamWriter(stream) { AutoFlush = true };
            var taskToken = new TaskCompletionSource<bool>();
            while (!httpContext.RequestAborted.IsCancellationRequested)
            {
                UpdateLoop();

                await taskToken.Task;
            }

            void UpdateLoop()
            {

            }
        }


        async Task Emit(StreamWriter writer, string emission)
        {
            Log.Information(emission);
            await writer.WriteLineAsync($"data: {emission}");
            await writer.FlushAsync();
        }
    }
}
