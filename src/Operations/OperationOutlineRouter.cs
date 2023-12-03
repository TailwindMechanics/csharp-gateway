//path: src\Operations\OperationOutlineRouter.cs

using Neurocache.ShipsInfo;
using Neurocache.Schema;

namespace Neurocache.Operations
{
    public static class OperationReportRouter
    {
        public static List<OperationReport> NextReport(OperationOutline outline, OperationReport previousReport)
        {
            var result = new List<OperationReport>();

            // Find the Current Node
            var currentNode = outline.Nodes.FirstOrDefault(n => n.Id == previousReport.ReportId.ToString());
            if (currentNode == null)
            {
                Ships.Log($"NextReport: Node with ID {previousReport.ReportId} not found.");
                return result;
            }

            Ships.Log($"NextReport: Found current node with ID {currentNode.Id}.");

            // Check the Node's Output Handles
            var sourceHandles = currentNode.Data.Handles.Where(h => h.Type == "source").ToList();
            if (!sourceHandles.Any())
            {
                Ships.Log($"NextReport: No output (source) handles found for node {currentNode.Id}.");
            }
            else
            {
                Ships.Log($"NextReport: Found {sourceHandles.Count} output (source) handle(s) for node {currentNode.Id}.");
            }

            // Find Outgoing Connections
            foreach (var handle in sourceHandles)
            {
                Ships.Log($"NextReport: Checking connections from handle {handle.Id} of node {currentNode.Id}.");

                var targetNodeIds = outline.Edges
                    .Where(e => e.Source == currentNode.Id && e.SourceHandle == handle.Id)
                    .Select(e => e.Target)
                    .ToList();

                if (!targetNodeIds.Any())
                {
                    Ships.Log($"NextReport: No target nodes found for source handle ID {handle.Id} of node {currentNode.Id}.");
                }
                else
                {
                    Ships.Log($"NextReport: Found {targetNodeIds.Count} target node(s) for source handle ID {handle.Id} of node {currentNode.Id}.");
                }

                // Identify Next Nodes
                foreach (var targetNodeId in targetNodeIds)
                {
                    var targetNode = outline.Nodes.FirstOrDefault(n => n.Id == targetNodeId);
                    if (targetNode == null)
                    {
                        Ships.Log($"NextReport: Target node with ID {targetNodeId} not found.");
                        continue;
                    }

                    // Create Reports for Next Nodes
                    var newReport = new OperationReport(
                        previousReport.Token,
                        Ships.ThisVessel,
                        targetNode.NodeType!,
                        previousReport.Payload,
                        previousReport.AgentId,
                        false,
                        targetNode.Id!
                    );

                    result.Add(newReport);
                    Ships.Log($"NextReport: Created new report for target node ID {targetNode.Id}.");
                }
            }

            // Return the Reports
            if (result.Any())
            {
                Ships.Log($"NextReport: Generated {result.Count} new report(s) for processing.");
            }
            else
            {
                Ships.Log("NextReport: No new reports generated for further processing.");
            }

            return result;
        }

    }
}
