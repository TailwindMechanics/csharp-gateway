//path: src\Operations\HubChannel.cs

using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Reactive;

using Neurocache.ConduitFrigate;
using Neurocache.ShipsInfo;
using Neurocache.Schema;

namespace Neurocache.Operations
{
    public class HubChannel(Guid agentid)
    {
        public readonly ISubject<OperationReport> SendReport = new Subject<OperationReport>();
        readonly Subject<OperationReport> onReportReceived = new();
        public IObservable<OperationReport> OnReportReceived
            => onReportReceived;

        readonly Subject<Unit> stop = new();
        IDisposable? downlinkSub;
        IDisposable? uplinkSub;

        public void Start()
        {
            uplinkSub = SendReport
                .ObserveOn(Scheduler.Default)
                .TakeUntil(stop)
                .Subscribe(operationReport =>
                {
                    Ships.Log($"Sending operation report: {operationReport}");
                    Conduit.Uplink(agentid.ToString(), operationReport, CancellationToken.None);
                });

            downlinkSub = Conduit.Downlink(agentid.ToString(), Conduit.DownlinkConsumer, CancellationToken.None)
                .ObserveOn(Scheduler.Default)
                .TakeUntil(stop)
                .Subscribe(operationReport =>
                {
                    Ships.Log($"Received operation report: {operationReport}");
                    onReportReceived.OnNext(operationReport);
                });

            Ships.Log("Starting hub channel");
        }

        public void Stop()
        {
            stop.OnNext(Unit.Default);
            downlinkSub?.Dispose();
            uplinkSub?.Dispose();
        }
    }
}
