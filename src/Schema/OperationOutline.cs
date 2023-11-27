//path: src\Schema\OperationOutline.cs

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
    }

    public class Handle
    {
        public string? Id;
        public string? HandleType;
        public float Angle { get; set; }
        public record Offset(float X, float Y);
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
    }

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
