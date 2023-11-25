//path: src\Schema\OperationData.cs

namespace Neurocache.Schema
{
    public record OperationReport(string Token, string Author, string Payload, bool Final, List<string> Dependents);
    public record OperationRequestData(string AgentId, string Prompt);
    public record StartAgentRequest(string AgentId, string Prompt);
    public record StopAgentRequest(string SessionToken);
    public record Ship(string Name, int Port);
    public record OperationToken(Guid Token);
}