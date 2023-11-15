//path: controllers\Agent\Schema\StartAgentRequest.cs

namespace Neurocache.Gateway.Controllers.Agent.Schema
{
    public record StartAgentRequest(string AgentId, string Prompt);
}
