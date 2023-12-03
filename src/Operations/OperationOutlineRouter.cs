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

            // Find the node corresponding to the previous report's ID.
            var currentNode = outline.Nodes.FirstOrDefault(n => n.Id == previousReport.ReportId.ToString());
            if (currentNode == null)
            {
                Ships.Warning($"NextReport: No node found for report ID {previousReport.ReportId}");
                return result;
            }

            Ships.Log($"NextReport: Found node for report ID {previousReport.ReportId}");

            // Process each handle of the current node.
            foreach (var handle in currentNode.Data.Handles)
            {
                // Find all target node IDs connected to the current handle.
                var targetNodeIds = outline.Edges
                    .Where(e => e.Source == handle.Id)
                    .Select(e => e.Target)
                    .ToList();

                if (!targetNodeIds.Any())
                {
                    Ships.Log($"NextReport: No target nodes found for handle ID {handle.Id}");
                    continue;
                }

                // Retrieve the actual node data for each target node ID.
                foreach (var targetNodeId in targetNodeIds)
                {
                    var targetNode = outline.Nodes.FirstOrDefault(n => n.Id == targetNodeId);
                    if (targetNode == null)
                    {
                        Ships.Warning($"NextReport: No node found for target node ID {targetNodeId}");
                        continue;
                    }

                    Ships.Log($"NextReport: Creating report for target node ID {targetNodeId}");
                    result.Add(new OperationReport(
                        previousReport.Token,
                        Ships.ThisVessel,
                        targetNode.NodeType!,
                        previousReport.Payload,
                        previousReport.AgentId,
                        false,
                        targetNode.Id!
                    ));
                }
            }

            return result;
        }
    }
}
