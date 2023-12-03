//path: src\Schema\OperationOutline.cs

using Newtonsoft.Json;

namespace Neurocache.Schema
{
    public record OperationOutline(Node[] Nodes, Edge[] Edges, Viewport? Viewport);

    public record Viewport(float X, float Y, float Zoom);

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

        public override string ToString()
            => JsonConvert.SerializeObject(this);
    }

    public class Handle
    {
        public string? Id;
        public string? Type;
        public float Angle { get; set; }
        public record Offset(float X, float Y);

        public override string ToString()
            => JsonConvert.SerializeObject(this);
    }

    public class Edge
    {
        public string? Id;
        public string? EdgeType;
        public string? Source;
        public string? Target;
        public int ZIndex;
        public string? SourceHandle;
        public string? TargetHandle;

        public override string ToString()
            => JsonConvert.SerializeObject(this);
    }

    public class NodeData
    {
        public string? Body;
        public string? NodeId;
        public List<Handle> Handles = new();
        public string? NodeName;
        public string? NodeType;
        public record NodePosition(float X, float Y);

        public override string ToString()
            => JsonConvert.SerializeObject(this);
    }
}
