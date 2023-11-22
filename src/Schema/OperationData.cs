//path: src\Schema\OperationData.cs

namespace Neurocache.Schema
{
    public record OperationReport(string Token, string Author, string Payload, bool Final, List<string> Dependents);
    public record OperationRequestData(string AgentId, string Prompt);
    public record OperationToken(Guid Token);
}
