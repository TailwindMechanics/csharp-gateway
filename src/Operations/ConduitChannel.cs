//path: src\Operations\ConduitChannel.cs

using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Reactive;

using Neurocache.ConduitFrigate;
using Neurocache.ShipsInfo;
using Neurocache.Schema;

namespace Neurocache.Operations
{
    public class ConduitChannel(Guid agentid)
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
                    Ships.Log($"ConduitChannel: Sending operation report: {operationReport}");
                    Conduit.Uplink(agentid.ToString(), operationReport, CancellationToken.None);
                });

            downlinkSub = Conduit.Downlink(agentid.ToString(), Conduit.DownlinkConsumer, CancellationToken.None)
                .ObserveOn(Scheduler.Default)
                .TakeUntil(stop)
                .Subscribe(operationReport =>
                {
                    Ships.Log($"ConduitChannel: Received operation report: {operationReport}");
                    onReportReceived.OnNext(operationReport);
                });

            Ships.Log("ConduitChannel: Started");
        }

        public void Stop()
        {
            stop.OnNext(Unit.Default);
            downlinkSub?.Dispose();
            uplinkSub?.Dispose();

            Ships.Log("ConduitChannel: Stopped");
        }
    }
}
