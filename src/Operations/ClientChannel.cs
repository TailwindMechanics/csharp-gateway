//path: src\Operations\ClientChannel.cs

using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive;
using Newtonsoft.Json;
using System.Text;

using Neurocache.ShipsInfo;
using Neurocache.Schema;

namespace Neurocache.Operations
{
    public class ClientChannel(WebSocket webSocket, Guid operationToken)
    {
        public readonly ISubject<OperationReport> SendReport = new Subject<OperationReport>();
        readonly Subject<OperationReport> onReportReceived = new();
        public IObservable<OperationReport> OnReportReceived
            => onReportReceived;

        readonly Subject<Unit> stop = new();
        readonly Guid operationToken = operationToken;
        readonly WebSocket webSocket = webSocket;
        IDisposable? sendReportSub;

        public async Task Start()
        {
            sendReportSub = SendReport
                .Where(_ => webSocket.State != WebSocketState.Open)
                .Select(JsonConvert.SerializeObject)
                .Where(json => !string.IsNullOrEmpty(json))
                .ObserveOn(Scheduler.Default)
                .TakeUntil(stop)
                .Subscribe(async jsonReport =>
                {
                    Ships.Log($"Sending message on operation token {operationToken}: {jsonReport}");
                    var messageBuffer = Encoding.UTF8.GetBytes(jsonReport);
                    await webSocket.SendAsync(
                        new ArraySegment<byte>(messageBuffer),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None
                    );
                });

            Ships.Log($"Starting client channel for operation token: {operationToken}");

            await ChannelLoop();
        }

        void OnMessageReceived(string message)
        {
            Ships.Log($"Message received on operation token {operationToken}: {message}");
            var report = OperationReport.FromJson(message);
            if (report == null)
            {
                Ships.Warning($"Invalid operation report received on operation token {operationToken}: {message}");
                return;
            }

            onReportReceived.OnNext(report);
        }

        public void Stop()
        {
            Ships.Log($"Stopping client channel for operation token: {operationToken}");
            webSocket.Abort();
            stop.OnNext(Unit.Default);
            sendReportSub?.Dispose();
        }

        async Task ChannelLoop()
        {
            var buffer = new byte[1024 * 4];
            try
            {
                WebSocketReceiveResult result;

                do
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Text && !result.CloseStatus.HasValue)
                    {
                        var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Ships.Log($"Message received on operation token {operationToken}: {receivedMessage}");
                        OnMessageReceived(receivedMessage);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Ships.Log($"WebSocket closing received for operation token {operationToken}");
                    }
                }
                while (!result.CloseStatus.HasValue);

                Ships.Log($"Starting close handshake for operation token {operationToken}");
                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
            catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
            {
                Ships.Log($"WebSocket connection closed prematurely for operation token {operationToken}: {ex.Message}");
            }
            finally
            {
                Ships.Log($"Cleaning up WebSocket for operation token {operationToken}");
            }

            Ships.Log("WebSocket connection closed for operation token: " + operationToken);
        }
    }
}
