//path: src\Controllers\OperationChannel.cs

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
        }

        public OperationChannel(
            IConsumer<string, OperationReport> downlink,
            CancellationTokenSource cancelToken,
            OperationToken operationToken,
            string agentId)
        {
            this.cancelToken = cancelToken;

            downlinkSub = Conduit.Downlink(agentId, downlink, cancelToken.Token)
                .Where(operationReport => operationReport.Token == operationToken.Token.ToString())
                .Subscribe(OnReportReceived);
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;
            response.ContentType = "text/event-stream";

            await using var stream = response.Body;
            writer = new StreamWriter(stream) { AutoFlush = true };
            var taskToken = new TaskCompletionSource<bool>();
            var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(
                context.HttpContext.RequestAborted,
                cancelToken.Token
            ).Token;
            while (!combinedToken.IsCancellationRequested)
            {
                await taskToken.Task.WaitAsync(combinedToken);
            }
        }

        void OnReportReceived(OperationReport operationReport)
        {
            Ships.Log($"Operation report received: {operationReport}");
            writer?.WriteLine($"data: {operationReport}");
        }
    }
}
