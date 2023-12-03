//path: src\Controllers\Controller.cs

using Microsoft.AspNetCore.Mvc;
using Supabase;

using Neurocache.CentralIntelFrigate;
using Neurocache.Operations;
using Neurocache.ShipsInfo;
using Neurocache.Utilities;
using System.Reactive;

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

            OperationService.KillAll();
            return Ok();
        }

        [HttpPost("agent/stop")]
        public IActionResult Stop([FromQuery] string operationToken)
        {
            if (!Keys.Guard(Request)) return Unauthorized();

            Ships.Log("Stopping operation");

            var tokenGuid = Guid.Parse(operationToken);
            if (tokenGuid == Guid.Empty)
            {
                Ships.Log("Token is invalid");
                return BadRequest("Token is invalid");
            }

            OperationService.Stop(tokenGuid);
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

            var agentIdGuid = Guid.Parse(agentId);
            if (agentIdGuid == Guid.Empty)
            {
                Ships.Log("Agent id is invalid");
                return BadRequest("Agent id is invalid");
            }

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

            Ships.Log($"Operation outline received, accepting web socket connection");
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            var operation = OperationService.CreateOperation(webSocket, agentIdGuid, operationOutline);
            await operation.Start();
            await operation.UpdateLoop();

            OperationService.Stop(operation.OperationToken);
            return new EmptyResult();
        }
    }
}
