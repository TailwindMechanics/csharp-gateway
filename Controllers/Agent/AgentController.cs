//path: controllers\Agent\AgentController.cs

using Microsoft.AspNetCore.Mvc;
using KafkaFlow.Producers;
using KafkaFlow;
using Supabase;
using Serilog;

using Neurocache.Gateway.Utilities;
using Neurocache.Gateway.Schema;

namespace Neurocache.Gateway.Controllers.Agent
{
    [ApiController]
    [Route("[controller]")]
    public class AgentController : ControllerBase
    {
        private readonly IMessageProducer producer;
        private readonly Client supabaseClient;

        public AgentController(Client supabaseClient, IProducerAccessor producerAccessor)
        {
            this.supabaseClient = supabaseClient;
            producer = producerAccessor.GetProducer(KafkaUtils.ProducerName);
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartAgent([FromBody] StartAgentRequest body)
        {
            Log.Information("StartAgent called with body: {Body}", body);

            if (!Keys.Guard(Request, out var apiKey))
            {
                Log.Warning("Unauthorized access attempt in StartAgent");
                return Unauthorized();
            }

            body.Deconstruct(out var agentId, out var prompt);
            Log.Information("Fetching graph for Agent ID: {AgentId}", agentId);
            var graph = await AgentUtils.GetAgentGraph(supabaseClient, agentId, apiKey);

            Log.Information("Producing message to Kafka for Agent ID: {AgentId}", agentId);
            await producer.ProduceAsync(
                KafkaUtils.TopicName,
                agentId,
                graph
            );

            var startMessage = agentId.StartMessage(graph!.Nodes[0].Data.Body!);
            Log.Information("StartAgent completed. Response: {StartMessage}", startMessage);
            return Ok(startMessage);
        }

        [HttpGet("stream")]
        public IActionResult StreamAgent([FromQuery] StreamAgentRequest query)
        {
            if (!Keys.Guard(Request, out var apiKey))
                return Unauthorized();

            query.Deconstruct(out var instanceId, out var outputLevel);
            return Ok(instanceId.StreamMessage(outputLevel));
        }

        [HttpPost("stop")]
        public IActionResult StopAgent([FromBody] StopAgentRequest body)
        {
            if (!Keys.Guard(Request, out var apiKey))
                return Unauthorized();

            body.Deconstruct(out var instanceId);
            return Ok(instanceId.StopMessage());
        }
    }
}
