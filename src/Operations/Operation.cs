//path: src\Operations\Operation.cs

using System.Reactive.Subjects;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive;

using Neurocache.RequestsChannel;
using Neurocache.ConduitFrigate;
using Neurocache.ShipsInfo;
using Neurocache.Schema;

namespace Neurocache.Operations
{
    public class Operation
    {
        public readonly Subject<Unit> StopSubject = new();
        public Guid OperationToken { get; }

        readonly ConduitChannel conduitChannel;
        readonly ClientChannel clientChannel;
        readonly OperationOutline outline;
        readonly Guid agentId;

        IDisposable? conduitReportSub;
        IDisposable? channelClosedSub;
        IDisposable? clientReportSub;
        IDisposable? stopSub;

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

            stopSub = StopSubject.Take(1)
                .Subscribe(_ => Stop());

            channelClosedSub = Conduit.ChannelClosed
                .Take(1)
                .TakeUntil(StopSubject)
                .Subscribe(_ => StopSubject.OnNext(Unit.Default));

            conduitReportSub = conduitChannel.OnReportReceived
                .Where(ValidConduitAuthor)
                .TakeUntil(StopSubject)
                .Subscribe(OnConduitReport);

            conduitChannel.Start();

            clientReportSub = clientChannel.OnReportReceived
                .Where(report => report.Token == OperationToken)
                .Where(ValidClientAuthor)
                .TakeUntil(StopSubject)
                .Subscribe(OnClientReport);

            clientChannel.Start();

            Ships.Log($"Operation started with token {OperationToken}");
            DispatchVanguardStarted();
        }

        public async Task UpdateLoop()
            => await clientChannel.UpdateLoop();

        void Stop()
        {
            Ships.Log($"Stopping operation with token {OperationToken}");

            DispatchStopReport();

            clientChannel.Stop();
            conduitChannel.Stop();

            stopSub?.Dispose();
            channelClosedSub?.Dispose();
            conduitReportSub?.Dispose();
            clientReportSub?.Dispose();
        }

        void OnReportReceived(OperationReport inboundReport)
        {
            Ships.Log($"OnReportReceived: {inboundReport}");

            var outboundReport = inboundReport;
            outboundReport.SetVanguardAuthor();
            DispatchReport(outboundReport);

            if (!outboundReport.Final)
            {
                Ships.Log($"Report is not final, will not route next: {outboundReport}");
                return;
            }

            var nextReports = OperationReportRouter.NextReport(outline, outboundReport);
            if (nextReports.Count == 0)
            {
                Ships.Log($"No next reports for {outboundReport}");
                return;
            }

            foreach (var nextReport in nextReports)
            {
                Ships.Log($"Dispatching next report: {nextReport}");
                DispatchReport(nextReport);
            }
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
            OnReportReceived(report);
        }

        void OnConduitReport(OperationReport report)
        {
            Ships.Log($"OnConduitReport: {report}");
            OnReportReceived(report);
        }

        void DispatchVanguardStarted()
        {
            var report = new OperationReport(
                OperationToken,
                Ships.ThisVessel,
                "All",
                "Vanguard started",
                agentId,
                false,
                "vanguard_started"
            );

            Ships.Log($"Dispatching Vanguard started report: {report}");
            DispatchReport(report);

            Ships.Log($"Uplinking Vanguard started to requests channel: {report}");
            RequestsChannelService.UplinkReport.OnNext(report);
        }

        void DispatchStopReport()
        {
            var report = new OperationReport(
                OperationToken,
                Ships.ThisVessel,
                "All",
                "Vanguard stopped",
                agentId,
                true,
                "vanguard_stopped"
            );

            Ships.Log($"Dispatching Vanguard stopped report: {report}");
            DispatchReport(report);
        }

        bool ValidClientAuthor(OperationReport report)
            => report.Author == "Client";

        bool ValidConduitAuthor(OperationReport report)
            => report.Author != "Client"
                && report.Author != "Vanguard";
    }
}
