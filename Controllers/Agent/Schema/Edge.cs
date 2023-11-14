//path: controllers\Agent\Schema\Edge.cs

namespace neurocache_gateway.Controllers.Agent.Schema
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
