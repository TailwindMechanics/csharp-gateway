//path: src\Operations\OperationOutlineRouter.cs

using Neurocache.ShipsInfo;
using Neurocache.Schema;

namespace Neurocache.Operations
{
    public static class OperationReportRouter
    {
        public static List<OperationReport> NextReport(OperationOutline outline, OperationReport previousReport)
        {
            var dependents = new List<Node>();
            var prevNode = outline.Nodes
                .FirstOrDefault(n => n.Id == previousReport.ReportId.ToString());

            if (prevNode == null)
            {
                Ships.Warning($"OperationOutlineRouter: Node with id {previousReport.ReportId} not found");
                return [];
            }

            foreach (var handle in prevNode.Data.Handles)
            {
                var nextNodeIds = outline.Edges
                  .Where(e => e.Source == handle.Id)
                  .Select(e => e.Target)
                  .ToList();

                var nextNodes = outline.Nodes.Where(n => nextNodeIds.Contains(n.Id));

                foreach (var nextNode in nextNodes)
                {
                    if (nextNode == null) continue;

                    dependents.Add(nextNode);
                }
            }

            var result = new List<OperationReport>();
            foreach (var node in dependents)
            {
                if (node == null || node.NodeType == null || node.Id == null) continue;

                result.Add(new OperationReport(
                    previousReport.Token,
                    Ships.ThisVessel,
                    node.NodeType,
                    previousReport.Payload,
                    previousReport.AgentId,
                    false,
                    node.Id
                ));
            }

            return result;
        }
    }
}
