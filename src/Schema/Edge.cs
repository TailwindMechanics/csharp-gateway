//path: src\Schema\Edge.cs

namespace Neurocache.Gateway.Schema
{
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
}
