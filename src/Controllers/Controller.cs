//path: src\Controllers\Controller.cs

using Microsoft.AspNetCore.Mvc;
using Confluent.Kafka;

using Neurocache.OperationChannels;
using Neurocache.ShipsInfo;
using Neurocache.Utilities;
using Neurocache.Schema;

namespace Neurocache.Controllers
{
    [ApiController]
    public class Controller(
        IConsumer<string, OperationReport> downlink
    ) : ControllerBase
    {
        [HttpPost("agent/kill")]
        public IActionResult Kill()
        {
            if (!Keys.Guard(Request)) return Unauthorized();

            Ships.Log("Killing all operations");

            OperationChannelService.Instance.KillAll();
            return Ok();
        }

        [HttpPost("agent/stop")]
        public IActionResult Stop([FromBody] StopAgentRequest body)
        {
            if (!Keys.Guard(Request)) return Unauthorized();

            Ships.Log("Stopping operation");

            var tokenGuid = Guid.Parse(body.SessionToken);
            OperationChannelService.Instance.StopOperationChannel(tokenGuid);
            return Ok();
        }

        [HttpPost("agent/run")]
        public async Task<IActionResult> Run([FromBody] OperationRequestData operationRequest)
        {
            Ships.Log($"Received operation request: {operationRequest}");
            if (!Keys.Guard(Request, out var apiKey)) return Unauthorized();

            Ships.Log($"Received request: {Request}");
            Ships.Log($"Received key: {apiKey}");

            var operationToken = await Vanguard.Notice.OperationRequest(apiKey, operationRequest);
            Ships.Log($"Received operation token: {operationToken}");
            if (operationToken is null) return Unauthorized();

            Ships.Log("Starting operation channel");
            return OperationChannelService.Instance.StartOperationChannel(
                downlink,
                operationToken,
                operationRequest.AgentId
            );
        }
    }
}
