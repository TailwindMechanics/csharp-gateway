//path: src\Operations\Operation.cs

using System.Net.WebSockets;
using System.Reactive.Linq;

using Neurocache.ConduitFrigate;
using Neurocache.ShipsInfo;
using Neurocache.Schema;

namespace Neurocache.Operations
{
    public class Operation
    {
        public Guid OperationToken { get; }

        readonly ConduitChannel conduitChannel;
        readonly ClientChannel clientChannel;
        readonly OperationOutline outline;
        readonly Guid agentId;

        public Operation(
            WebSocket webSocket,
            Guid agentId,
            OperationOutline outline
        )
        {
            this.outline = outline;
            OperationToken = Guid.NewGuid();
            conduitChannel = new ConduitChannel(agentId);
            clientChannel = new ClientChannel(webSocket, OperationToken);
            this.agentId = agentId;
        }

        public async Task Start()
        {
            await Conduit.EnsureTopicExists(agentId.ToString());

            conduitChannel.OnReportReceived
                .Where(ValidConduitAuthor)
                .Subscribe(OnConduitReport);
            conduitChannel.Start();

            clientChannel.OnReportReceived
                .Where(report => report.Token == OperationToken)
                .Where(ValidClientAuthor)
                .Subscribe(OnClientReport);
            clientChannel.Start();

            await clientChannel.UpdateLoop();
        }

        public void Stop()
        {
            clientChannel.Stop();
            conduitChannel.Stop();
        }

        void OnClientReport(OperationReport report)
        {
            Ships.Log($"OnClientReport: {report}");
            conduitChannel.SendReport.OnNext(report);
        }

        void OnConduitReport(OperationReport report)
        {
            Ships.Log($"OnConduitReport: {report}");
            clientChannel.SendReport.OnNext(report);
        }

        bool ValidClientAuthor(OperationReport report)
            => report.Author == "Client";

        bool ValidConduitAuthor(OperationReport report)
            => report.Author == "Client"
                || report.Author == "Hub";
    }
}
