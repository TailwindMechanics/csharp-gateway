//path: src\Controllers\Controller.cs

using Microsoft.AspNetCore.Mvc;
using Confluent.Kafka;
using Supabase;

using Neurocache.CentralIntelFrigate;
using Neurocache.OperationChannels;
using Neurocache.ConduitFrigate;
using Neurocache.ShipsInfo;
using Neurocache.Utilities;
using Neurocache.Schema;

namespace Neurocache.Controllers
{
    [ApiController]
    public class Controller(
        Client supabaseClient,
        IProducer<string, OperationReport> uplink,
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


            // --== Old Vanguard ==--
            Ships.Log($"Operation request received, key: {apiKey}");
            var operationOutline = await CentralIntel.OperationRequest(
                operationRequest.AgentId,
                supabaseClient,
                apiKey
            );

            if (operationOutline == null)
            {
                Ships.Warning("Operation outline is null");
                return Unauthorized();
            }

            await Conduit.EnsureTopicExists(operationRequest.AgentId);
            var operationToken = new OperationToken(Guid.NewGuid());
            // --== Old Vanguard ==--


            Ships.Log("Starting operation channel");
            return OperationChannelService.Instance.StartOperationChannel(
                downlink,
                operationToken,
                operationRequest.AgentId
            );
        }
    }
}
