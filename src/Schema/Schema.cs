//path: src\Schema\Schema.cs

namespace Neurocache.Schema
{
    public record OperationRequestData(string AgentId, string Prompt);
    public record StartAgentRequest(string AgentId, string Prompt);
    public record StopAgentRequest(string SessionToken);
    public record Ship(string Name, int Port);
    public record OperationToken(Guid Token);
}
