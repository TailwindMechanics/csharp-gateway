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

            var currentNode = outline.Nodes.FirstOrDefault(n => n.Id == previousReport.ReportId.ToString());
            if (currentNode == null)
            {
                Ships.Warning($"NextReport: No node found for report ID {previousReport.ReportId}");
                return result;
            }

            Ships.Log($"NextReport: Found node for report ID {previousReport.ReportId}");

            foreach (var handle in currentNode.Data.Handles.Where(h => h.HandleType == "source"))
            {
                var nextNodeIds = outline.Edges
                    .Where(e => e.Source == currentNode.Id && e.SourceHandle == handle.Id)
                    .Select(e => e.Target)
                    .ToList();

                if (!nextNodeIds.Any())
                {
                    Ships.Log($"NextReport: No next nodes found for source handle ID {handle.Id}");
                    continue;
                }

                foreach (var targetNodeId in nextNodeIds)
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
