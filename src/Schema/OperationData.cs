//path: src\Schema\OperationRequestData.cs

namespace Neurocache.Schema
{
    public record OperationRequestData(string AgentId, string Prompt);
    public record OperationToken(Guid Token);
}
