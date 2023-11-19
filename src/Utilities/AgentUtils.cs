//path: src\Utilities\AgentUtils.cs

using Postgrest.Responses;
using Newtonsoft.Json;
using Supabase;
using Serilog;

using Neurocache.Schema;

namespace Neurocache.Utilities
{
    public static class AgentUtils
    {
        public static async Task<AgentGraphData?> GetAgentGraph(Client client, string agentId, Guid userApiKey)
        {
            var parameters = new Dictionary<string, object>
            {
                { "agentid", agentId },
                { "userkey", userApiKey }
            };

            var response = await client.Rpc("get_agent_graph", parameters);
            if (!IsValidResponse(response, out var content)) return null;
            if (!IsValidGraph(content, out var graph)) return null;

            return graph;
        }

        static bool IsValidResponse(BaseResponse response, out string content)
        {
            if (response == null || response.Content == null || response.ResponseMessage == null || !response.ResponseMessage.IsSuccessStatusCode)
            {
                Log.Error("Error getting agent graph, invalid res: {error}", JsonConvert.SerializeObject(response));
                content = "";
                return false;
            }

            content = response.Content;
            return true;
        }

        static bool IsValidGraph(string content, out AgentGraphData? graph)
        {
            try
            {
                graph = JsonConvert.DeserializeObject<AgentGraphData>(content);
                return graph != null;
            }
            catch (JsonException ex)
            {
                Log.Error("Error deserializing agent graph: {error}", ex.Message);
                graph = null;
                return false;
            }
        }
    }
}
