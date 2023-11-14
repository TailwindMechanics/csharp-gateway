//path: controllers\Agent\Schema\Handle.cs

namespace neurocache_gateway.Controllers.Agent.Schema
{
    public class Handle
    {
        public string? Id;
        public string? HandleType;
        public float Angle { get; set; }
        public record Offset(float X, float Y);
    }
}
