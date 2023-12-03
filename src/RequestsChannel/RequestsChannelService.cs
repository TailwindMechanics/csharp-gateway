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

        static IDisposable? channelClosedSub;
        const double restartDelaySeconds = 1;
        static IDisposable? downlinkSub;
        static IDisposable? uplinkSub;
        static bool starting;

        public static void OnAppStart()
        {
            starting = true;

            channelClosedSub = Conduit
                .ChannelClosed
                .Where(_ => !starting)
                .ObserveOn(Scheduler.Default)
                .Subscribe(_ => Stop(true));

            Start();
        }

        static async void Start()
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
                .Where(report => report != null)
                .Subscribe(operationReport =>
                {
                    Ships.Log($"Received operation report: {operationReport}");
                    onDownlinkReceived.OnNext(operationReport!);
                });

            Ships.Log("RequestsChannelService started");
            starting = false;
        }

        static async void Stop(bool restart)
        {
            Ships.Log("RequestsChannelService stopping");
            channelClosedSub?.Dispose();
            downlinkSub?.Dispose();
            uplinkSub?.Dispose();

            if (!restart) return;

            starting = true;
            Ships.Log($"Restarting in {restartDelaySeconds} seconds");

            await Task.Delay(TimeSpan.FromSeconds(restartDelaySeconds));

            Start();
        }

        public static void OnAppClosing()
            => Stop(false);
    }
}
