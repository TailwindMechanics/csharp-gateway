//path: src\Operations\Operation.cs

using System.Net.WebSockets;
using System.Reactive.Linq;

using Neurocache.RequestsChannel;
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

            Ships.Log($"Operation started with token {OperationToken}");
            DispatchVanguardStarted();
        }

        public async Task UpdateLoop()
            => await clientChannel.UpdateLoop();

        public void Stop()
        {
            clientChannel.Stop();
            conduitChannel.Stop();
        }

        void OnReportReceived(OperationReport report)
        {
            Ships.Log($"OnReportReceived: {report}");

            if (!report.Final)
            {
                Ships.Log($"Report is not final, will not route next: {report}");
                return;
            }

            var nextReports = OperationReportRouter.NextReport(outline, report);
            if (nextReports.Count == 0)
            {
                Ships.Log($"No next reports for {report}");
                return;
            }

            foreach (var nextReport in nextReports)
            {
                Ships.Log($"Dispatching next report: {nextReport}");
                DispatchReport(nextReport);
            }
        }

        void DispatchVanguardStarted()
        {
            var report = new OperationReport(
                OperationToken,
                Ships.ThisVessel,
                agentId.ToString(),
                false,
                "vanguard_started"
            );

            Ships.Log($"Dispatching Vanguard started report: {report}");
            DispatchReport(report);

            Ships.Log($"Uplinking Vanguard started to requests channel: {report}");
            RequestsChannelService.UplinkReport.OnNext(report);
        }

        void DispatchReport(OperationReport report)
        {
            Ships.Log($"Dispatching report: {report}");
            clientChannel.SendReport.OnNext(report);
            conduitChannel.SendReport.OnNext(report);
        }

        void OnClientReport(OperationReport report)
        {
            Ships.Log($"OnClientReport: {report}");
            conduitChannel.SendReport.OnNext(report);
            OnReportReceived(report);
        }

        void OnConduitReport(OperationReport report)
        {
            Ships.Log($"OnConduitReport: {report}");
            clientChannel.SendReport.OnNext(report);
            OnReportReceived(report);
        }

        bool ValidClientAuthor(OperationReport report)
            => report.Author == "Client";

        bool ValidConduitAuthor(OperationReport report)
            => report.Author == "Client"
                || report.Author == "Hub";
    }
}
