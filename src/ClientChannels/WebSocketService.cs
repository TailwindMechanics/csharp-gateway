//path: src\ClientChannels\WebSocketService.cs

// path: src\Services\WebSocketService.cs

using System.Net.WebSockets;
using System.Text;

using Neurocache.ShipsInfo;

namespace Neurocache.ClientChannels
{
    public class WebSocketService
    {
        public static WebSocketService Instance => instance;
        readonly Dictionary<Guid, WebSocket> webSockets = [];
        static readonly WebSocketService instance = new();
        WebSocketService() { }

        public void AddWebSocket(Guid operationToken, WebSocket webSocket)
        {
            if (!webSockets.ContainsKey(operationToken))
            {
                webSockets[operationToken] = webSocket;
                Ships.Log($"WebSocket added for operation token: {operationToken}");
            }
        }

        public async Task SendMessageAsync(Guid operationToken, string message)
        {
            var webSocket = webSockets[operationToken];
            if (webSocket == null)
            {
                Ships.Log($"No WebSocket found for operation token: {operationToken}");
                return;
            }

            var buffer = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            Ships.Log($"Message sent to operation token: {operationToken}");
        }

        public void RemoveWebSocket(Guid operationToken)
        {
            var webSocket = webSockets[operationToken];
            if (webSocket == null)
            {
                Ships.Log($"No WebSocket found for operation token: {operationToken}");
                return;
            }

            webSockets.Remove(operationToken);
            webSocket.Dispose();
            Ships.Log($"WebSocket removed for operation token: {operationToken}");
        }
    }
}
