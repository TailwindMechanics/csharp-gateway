//path: controllers\Agent\Schema\AgentGraphData.cs

using Neurocache.Gateway.Controllers.Agent.Schema;

namespace Neurocache.Gateway.Controllers.Agent
{
    public partial class AgentController
    {
        public record AgentGraphData(Node[] Nodes, Edge[] Edges, Viewport? Viewport);
    }
}
