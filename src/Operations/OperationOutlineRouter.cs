//path: src\Operations\OperationOutlineRouter.cs

using Neurocache.ShipsInfo;
using Neurocache.Schema;

namespace Neurocache.Operations
{
    public static class OperationReportRouter
    {
        public static List<OperationReport> NextReport(OperationOutline outline, OperationReport previousReport)
        {
            var dependents = new List<string>();
            var prevNode = outline.Nodes
                .FirstOrDefault(n => n.Id == previousReport.ReportId.ToString());

            if (prevNode == null)
            {
                Ships.Warning($"OperationOutlineRouter: Node with id {previousReport.ReportId} not found");
                return [];
            }

            foreach (var handle in prevNode.Data.Handles)
            {
                var nextNodes = outline.Edges
                  .Where(e => e.Source == handle.Id)
                  .Select(e => e.Target)
                  .ToList();

                foreach (var nextNode in nextNodes)
                {
                    if (nextNode == null) continue;

                    dependents.Add(nextNode);
                }
            }

            var result = new List<OperationReport>();
            dependents.ForEach(id =>
            {
                result.Add(new OperationReport(
                  previousReport.Token,
                  Ships.ThisVessel,
                  previousReport.Payload,
                  false,
                  id
                ));
            });

            return result;
        }
    }
}
