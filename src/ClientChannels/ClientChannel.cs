//path: src\ClientChannels\ClientChannel.cs

using System.Net.WebSockets;
using System.Text;

using Neurocache.ShipsInfo;

namespace Neurocache.ClientChannels
{
    public class ClientChannel(WebSocket webSocket, Guid operationToken)
    {
        readonly Guid operationToken = operationToken;
        readonly WebSocket webSocket = webSocket;

        public async Task StartCommunication()
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result;

            do
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text && !result.CloseStatus.HasValue)
                {
                    var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    OnMessageReceived(receivedMessage);
                }
            }
            while (!result.CloseStatus.HasValue);

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);

            Ships.Log("WebSocket connection closed for operation token: " + operationToken);
        }

        public async Task SendMessageAsync(string message)
        {
            if (webSocket.State != WebSocketState.Open)
            {
                Ships.Warning($"WebSocket connection is not open for operation token: {operationToken}");
                return;
            }

            Ships.Log($"Sending message on operation token {operationToken}: {message}");
            var messageBuffer = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(
                new ArraySegment<byte>(messageBuffer),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
        }

        void OnMessageReceived(string message)
        {
            Ships.Log($"Message received on operation token {operationToken}: {message}");
        }
    }
}
