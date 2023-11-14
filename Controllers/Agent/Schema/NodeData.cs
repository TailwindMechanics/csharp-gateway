//path: controllers\Agent\Schema\NodeData.cs

namespace neurocache_gateway.Controllers.Agent.Schema
{
    public class NodeData
    {
        public string? Body;
        public string? NodeId;
        public List<Handle> Handles = new();
        public string? NodeName;
        public string? NodeType;
        public record NodePosition(float X, float Y);
    }
}
