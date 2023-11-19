//path: src\Schema\Handle.cs

namespace Neurocache.Schema
{
    public class Handle
    {
        public string? Id;
        public string? HandleType;
        public float Angle { get; set; }
        public record Offset(float X, float Y);
    }
}
