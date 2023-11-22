//path: src\Controllers\Controller.cs

using Microsoft.AspNetCore.Mvc;

using Neurocache.OperationChannels;
using Neurocache.Utilities;
using Neurocache.Schema;
using Confluent.Kafka;

namespace Neurocache.Controllers.Agent
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

            OperationChannelService.Instance.KillAll();
            return Ok();
        }

        [HttpPost("agent/stop")]
        public IActionResult StopAgent([FromBody] StopAgentRequest body)
        {
            if (!Keys.Guard(Request)) return Unauthorized();

            var tokenGuid = Guid.Parse(body.SessionToken);
            OperationChannelService.Instance.StopOperationChannel(tokenGuid);
            return Ok();
        }

        [HttpPost("agent/run")]
        public async Task<IActionResult> RunAgentAsync([FromBody] OperationRequestData operationRequest)
        {
            if (!Keys.Guard(Request, out var apiKey)) return Unauthorized();

            var operationToken = await Vanguard.Notice.OperationRequest(apiKey, operationRequest);
            if (operationToken is null) return Unauthorized();

            return OperationChannelService.Instance.StartOperationChannel(
                downlink,
                operationToken,
                operationRequest.AgentId
            );
        }
    }
}
