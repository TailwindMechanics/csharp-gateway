
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
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
            var cancelToken = new CancellationTokenSource();
            operations[operationToken.Token] = new OperationChannel(
                downlink,
                cancelToken,
                operationToken,
                agentId
            );
            return operations[operationToken.Token];
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
