//path: controllers\Agent\Schema\Node.cs

namespace Neurocache.Gateway.Controllers.Agent.Schema
{
    public class Node
    {
        public string? Id;
        public NodeData Data = new();
        public string? NodeType;
        public float Width;
        public float Height;
        public record Position(float X, float Y);
        public bool Selected { get; set; }
        public record PositionAbsolute(float X, float Y);
        public bool? Dragging;
    }
}
