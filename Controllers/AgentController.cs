//path: controllers\AgentController.cs

using Microsoft.AspNetCore.Mvc;

[ApiController]
public class AgentController : ControllerBase
{
    public record RunAgentRequest(string agentId, string prompt);

    [HttpPost("agent/run")]
    public IActionResult RunAgent(
        [FromHeader(Name = "apikey")] Guid apiKey,
        [FromBody] RunAgentRequest agentRequest)
    {
        Console.WriteLine($"Agent {agentRequest.agentId} is running");
        return Ok($"{agentRequest.agentId} is running");
    }
}
