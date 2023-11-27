//path: src\CentralIntelFrigate\CentralIntel.cs

using Postgrest.Responses;
using Newtonsoft.Json;
using Supabase;
using Serilog;

using Neurocache.ShipsInfo;
using Neurocache.Schema;

namespace Neurocache.CentralIntelFrigate
{
    public static class CentralIntel
    {
        public static Client CreateClient()
            => new(
                Environment.GetEnvironmentVariable("SUPABASE_URL")!,
                Environment.GetEnvironmentVariable("SUPABASE_KEY")!,
                new SupabaseOptions
                {
                    AutoRefreshToken = true,
                    AutoConnectRealtime = true
                }
            );

        public static async Task<OperationOutline?> OperationRequest(string agentId, Client client, Guid userApiKey)
        {
            var parameters = new Dictionary<string, object>
            {
                { "agentid", agentId },
                { "userkey", userApiKey }
            };

            Ships.Log($"Requesting operation for agent {agentId} with user key {userApiKey}");
            var response = await client.Rpc("get_agent_graph", parameters);
            Ships.Log($"Operation request response: {response.Content}");
            if (!IsValidResponse(response, out var content)) return null;
            if (!IsValidOperation(content, out var operationOutline)) return null;

            if (operationOutline == null)
            {
                Log.Warning($"Operation request denied.");
                return null;
            }

            Ships.Log($"Operation request accepted.");
            return operationOutline;
        }

        static bool IsValidResponse(BaseResponse response, out string content)
        {
            if (response == null || response.Content == null || response.ResponseMessage == null || !response.ResponseMessage.IsSuccessStatusCode)
            {
                content = "";
                return false;
            }

            content = response.Content;
            return true;
        }

        static bool IsValidOperation(string content, out OperationOutline? operationOutline)
        {
            operationOutline = JsonConvert.DeserializeObject<OperationOutline>(content);
            return operationOutline != null;
        }
    }
}
