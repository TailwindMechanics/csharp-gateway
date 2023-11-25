//path: src\OperationChannels\OperationChannel.cs

using Microsoft.AspNetCore.Mvc;
using System.Reactive.Linq;
using Confluent.Kafka;

using Neurocache.ConduitFrigate;
using Neurocache.ShipsInfo;
using Neurocache.Schema;

namespace Neurocache.OperationChannels
{
    public class OperationChannel : IActionResult
    {
        readonly CancellationTokenSource cancelToken;

        readonly IDisposable downlinkSub;
        StreamWriter? writer;

        public void Stop()
        {
            cancelToken.Cancel();
            downlinkSub.Dispose();
            writer?.Dispose();
        }

        public OperationChannel(
            IConsumer<string, OperationReport> downlink,
            CancellationTokenSource cancelToken,
            OperationToken operationToken,
            string agentId
        )
        {
            Ships.Log("OperationChannel/ start");
            this.cancelToken = cancelToken;

            downlinkSub = Conduit.Downlink(agentId, downlink, cancelToken.Token)
                .Where(operationReport => operationReport.Token == operationToken.Token.ToString())
                .Subscribe(OnReportReceived);

            Ships.Log($"OperationChannel/ downlinkSub: {downlinkSub}");
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;
            response.ContentType = "text/event-stream";

            await using var stream = response.Body;
            writer = new StreamWriter(stream);

            var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(
                context.HttpContext.RequestAborted,
                cancelToken.Token
            ).Token;

            await Task.Delay(Timeout.Infinite, combinedToken);

            if (writer != null)
            {
                await writer.DisposeAsync();
            }
        }

        void OnReportReceived(OperationReport operationReport)
        {
            Ships.Log($"Operation report received: {operationReport}");
            if (writer != null)
            {
                writer.WriteLine($"data: {operationReport}");
                writer.FlushAsync();
            }
        }
    }
}
