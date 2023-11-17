//path: src\Schema\NodeData.cs

namespace Neurocache.Gateway.Schema
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
