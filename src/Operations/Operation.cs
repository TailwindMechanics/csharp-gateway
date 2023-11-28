//path: src\Operations\Operation.cs

using System.Net.WebSockets;

using Neurocache.ShipsInfo;
using Neurocache.Schema;
using Neurocache.ConduitFrigate;

namespace Neurocache.Operations
{
    public class Operation
    {
        public Guid Token { get; }

        readonly ClientChannel clientChannel;
        readonly OperationOutline outline;
        readonly HubChannel hubChannel;
        readonly Guid agentId;

        public Operation(
            WebSocket webSocket,
            Guid agentId,
            OperationOutline outline
        )
        {
            this.outline = outline;
            Token = Guid.NewGuid();
            hubChannel = new HubChannel(agentId);
            clientChannel = new ClientChannel(webSocket, Token);
            this.agentId = agentId;
        }

        public async Task Start()
        {
            await Conduit.EnsureTopicExists(agentId);

            hubChannel.Start();
            await clientChannel.Start();
        }

        public void Stop()
        {
            clientChannel.Stop();
            hubChannel.Stop();
        }

        void RouteReport(OperationReport report)
        {
            if (report.Author == "client")
            {

            }
        }

        void OnClientReport(OperationReport report)
        {
            Ships.Log($"Client report: {report.Payload}");
        }

        void OnHubReport(OperationReport report)
        {
            Ships.Log($"Hub report: {report.Payload}");
        }

        OperationReport DependentReport(OperationReport previousReport)
        {
            var dispatchReport = new OperationReport(
                previousReport.Token,
                Ships.ThisVesselName,
                previousReport.Payload,
                false,
                []
            );

            var prevNodes = outline.Nodes
                .Where(node => node.Id == previousReport.Author)
                .ToList();

            foreach (var node in prevNodes)
            {
                foreach (var handle in node.Data.Handles)
                {
                    var nextNodes = outline.Edges
                        .Where(edge => edge.Source == handle.Id)
                        .Select(edge => edge.Target)
                        .ToList();

                    foreach (var nextNode in nextNodes)
                    {
                        var nextNodeData = outline.Nodes
                            .Where(nodeData => nodeData.Id == nextNode)
                            .Select(nodeData => nodeData.Data)
                            .ToList();

                        foreach (var data in nextNodeData)
                        {
                            if (data.NodeId != null && data.Body != null)
                            {
                                dispatchReport.Dependents.Add(data.NodeId);
                            }
                        }
                    }
                }
            }

            return dispatchReport;
        }
    }
}
