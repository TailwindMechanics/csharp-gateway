//path: src\ClientChannels\ClientChannelService.cs

using System.Collections.Concurrent;
using System.Net.WebSockets;

using Neurocache.ShipsInfo;

namespace Neurocache.ClientChannels
{
    public class ClientChannelService
    {
        public static ClientChannelService Instance => _instance;
        static readonly ConcurrentDictionary<Guid, ClientChannel> clientChannels = new();
        static readonly ClientChannelService _instance = new();
        ClientChannelService() { }

        public static ClientChannel CreateClientChannel(WebSocket webSocket, Guid operationToken)
        {
            Ships.Log($"Creating client channel for operation token: {operationToken}");
            var clientChannel = new ClientChannel(webSocket, operationToken);
            clientChannels.TryAdd(operationToken, clientChannel);
            return clientChannel;
        }

        public static void RemoveClientChannel(Guid operationToken)
        {
            if (!clientChannels.TryRemove(operationToken, out _))
            {
                Ships.Log($"No ClientChannel found for operation token: {operationToken}");
            }
        }
    }
}
