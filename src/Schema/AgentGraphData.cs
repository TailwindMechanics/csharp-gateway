//path: src\Schema\AgentGraphData.cs

namespace Neurocache.Schema
{
    public record AgentGraphData(Node[] Nodes, Edge[] Edges, Viewport? Viewport);
}
