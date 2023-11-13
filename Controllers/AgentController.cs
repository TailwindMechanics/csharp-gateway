using Microsoft.AspNetCore.Mvc;

[ApiController]
public class AgentController : ControllerBase
{
    public record RunAgentRequest(string AgentId, string Prompt);

    [HttpPost("agent/run")]
    public IActionResult RunAgent(
        [FromHeader(Name = "apikey")] Guid apiKey,
        [FromBody] RunAgentRequest agentRequest)
            => Ok($"{agentRequest.AgentId} is running with API key {apiKey}");
}
