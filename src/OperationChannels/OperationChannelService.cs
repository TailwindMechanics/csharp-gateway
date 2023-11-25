//path: src\OperationChannels\OperationChannelService.cs

using Microsoft.AspNetCore.Mvc;
using Confluent.Kafka;

using Neurocache.Schema;

namespace Neurocache.OperationChannels
{
    public class OperationChannelService
    {
        private static readonly OperationChannelService instance = new();
        readonly Dictionary<Guid, OperationChannel> operations = [];
        public static OperationChannelService Instance => instance;
        private OperationChannelService() { }

        public IActionResult StartOperationChannel(IConsumer<string, OperationReport> downlink, OperationToken operationToken, string agentId)
        {
            ShipsInfo.Ships.Log($"OperationChannelService.StartOperationChannel/ downlink: {downlink}");
            ShipsInfo.Ships.Log($"OperationChannelService.StartOperationChannel/ operationToken: {operationToken}");
            ShipsInfo.Ships.Log($"OperationChannelService.StartOperationChannel/ agentId: {agentId}");

            var cancelToken = new CancellationTokenSource();
            ShipsInfo.Ships.Log($"OperationChannelService.StartOperationChannel/ cancelToken: {cancelToken}");
            operations[operationToken.Token] = new OperationChannel(
                downlink,
                cancelToken,
                operationToken,
                agentId
            );

            var operationChannel = operations[operationToken.Token];
            ShipsInfo.Ships.Log($"OperationChannelService.StartOperationChannel/ operationChannel: {operationChannel}");
            return operationChannel;
        }

        public void StopOperationChannel(Guid operationToken)
        {
            operations[operationToken].Stop();
            operations.Remove(operationToken);
        }

        public void KillAll()
        {
            foreach (var operation in operations)
            {
                operation.Value.Stop();
            }
        }
    }
}
