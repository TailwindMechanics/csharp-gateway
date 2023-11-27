//path: src\Controllers\Controller.cs

using Microsoft.AspNetCore.Mvc;
using Supabase;

using Neurocache.CentralIntelFrigate;
using Neurocache.ClientChannels;
using Neurocache.ConduitFrigate;
using Neurocache.ShipsInfo;
using Neurocache.Utilities;
using Neurocache.Schema;

namespace Neurocache.Controllers
{
    [ApiController]
    public class WebSocketController(Client supabaseClient) : ControllerBase
    {
        [HttpPost("agent/kill")]
        public IActionResult Kill()
        {
            if (!Keys.Guard(Request)) return Unauthorized();

            Ships.Log("Killing all operations");

            ClientChannelService.KillAll();
            return Ok();
        }

        [HttpPost("agent/stop")]
        public IActionResult Stop([FromBody] StopAgentRequest body)
        {
            if (!Keys.Guard(Request)) return Unauthorized();

            Ships.Log("Stopping operation");

            var tokenGuid = Guid.Parse(body.SessionToken);
            ClientChannelService.Stop(tokenGuid);
            return Ok();
        }

        [HttpGet("agent/run")]
        public async Task<IActionResult> RunAgent([FromQuery] string agentId, [FromQuery] string apiKey)
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                Ships.Log("WebSocket request expected.");
                return BadRequest("WebSocket request expected.");
            }
            if (!Keys.Guard(apiKey, out var guidKey)) return Unauthorized();

            Ships.Log($"Operation request received, key: {guidKey}");
            var operationOutline = await CentralIntel.OperationRequest(
                agentId,
                supabaseClient,
                guidKey
            );

            if (operationOutline == null)
            {
                Ships.Warning("Operation outline is null");
                return Unauthorized();
            }

            await Conduit.EnsureTopicExists(agentId);
            var operationToken = new OperationToken(Guid.NewGuid());

            Ships.Log($"Operation outline received, token created, accepting web socket connection");
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            Ships.Log($"Creating client channel for operation token: {operationToken.Token}");
            var clientChannel = ClientChannelService.CreateClientChannel(webSocket, operationToken.Token);
            await clientChannel.StartCommunication();

            Ships.Log($"Removing client channel for operation token: {operationToken.Token}");
            ClientChannelService.RemoveClientChannel(operationToken.Token);
            return new EmptyResult();
        }
    }
}
