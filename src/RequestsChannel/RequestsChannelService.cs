//path: src\RequestsChannel\RequestsChannelService.cs

using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Reactive.Linq;

using Neurocache.ConduitFrigate;
using Neurocache.ShipsInfo;
using Neurocache.Schema;

namespace Neurocache.RequestsChannel
{
    public static class RequestsChannelService
    {
        public static readonly ISubject<OperationReport> UplinkReport = new Subject<OperationReport>();
        static readonly Subject<OperationReport> onDownlinkReceived = new();
        public static IObservable<OperationReport> OnDownlinkReceived
            => onDownlinkReceived;

        const string requestsTopic = "operation_requests";
        static IDisposable? downlinkSub;
        static IDisposable? uplinkSub;

        public static async void OnAppStart()
        {
            await Conduit.EnsureTopicExists(requestsTopic);

            uplinkSub = UplinkReport
                .ObserveOn(Scheduler.Default)
                .Subscribe(operationReport =>
                {
                    Ships.Log($"Sending operation report: {operationReport}");
                    Conduit.Uplink(requestsTopic, operationReport, CancellationToken.None);
                });

            downlinkSub = Conduit.Downlink(requestsTopic, Conduit.DownlinkConsumer, CancellationToken.None)
                .ObserveOn(Scheduler.Default)
                .Subscribe(operationReport =>
                {
                    Ships.Log($"Received operation report: {operationReport}");
                    onDownlinkReceived.OnNext(operationReport);
                });

            Ships.Log("RequestsChannelService started");
        }

        public static void OnAppClosing()
        {
            Ships.Log("RequestsChannelService closing");

            downlinkSub?.Dispose();
            uplinkSub?.Dispose();
        }
    }
}
