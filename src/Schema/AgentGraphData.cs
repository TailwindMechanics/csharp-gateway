//path: src\Schema\AgentGraphData.cs

namespace Neurocache.Gateway.Schema
{
    public record AgentGraphData(Node[] Nodes, Edge[] Edges, Viewport? Viewport);
}
