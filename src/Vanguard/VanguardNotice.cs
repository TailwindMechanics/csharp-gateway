//path: src\Vanguard\VanguardNotice.cs

using Newtonsoft.Json;
using System.Text;
using Serilog;

using Neurocache.ShipsInfo;
using Neurocache.Schema;

namespace Neurocache.Vanguard
{
    public static class Notice
    {
        static readonly HttpClient httpClient = new HttpClient();

        public static async Task<OperationToken?> OperationRequest(Guid apiKey, OperationRequestData operationRequest)
        {
            Ships.Log($"Notice.OperationRequest/ apiKey: {apiKey}");
            var request = VanguardNotice(
                JsonConvert.SerializeObject(operationRequest),
                "operation/request",
                apiKey
            );

            Ships.Log($"Notice.OperationRequest/ request: {request}");
            var response = await VanguardResponse(request);
            Ships.Log($"Notice.OperationRequest/ VanguardResponse: {response}");
            var content = await response.Content.ReadAsStringAsync();
            Ships.Log($"Notice.OperationRequest/ content: {content}");
            var operationToken = JsonConvert.DeserializeObject<OperationToken>(content);
            Ships.Log($"Notice.OperationRequest/ operationToken: {operationToken}");

            return operationToken;
        }

        static async Task<HttpResponseMessage> VanguardResponse(HttpRequestMessage request)
        {
            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                Log.Warning($"Notice.VanguardResponse/ Operation Request Rejected");
            }
            else
            {
                Log.Information($"Notice.VanguardResponse/ Operation Request Approved");
            }
            return response;
        }

        static HttpRequestMessage VanguardNotice(string payload, string endpoint, Guid apiKey)
        {
            endpoint = endpoint.StartsWith("/") ? endpoint[1..] : endpoint;
            var vanguardUrl = $"{Ships.VanguardAddress()}/{endpoint}";

            Ships.Log($"Notice.VanguardNotice/ vanguardUrl: {vanguardUrl}");

            var request = new HttpRequestMessage(HttpMethod.Post, vanguardUrl);

            Ships.Log($"Notice.VanguardNotice/ request: {request}");

            request.Headers.Add("apikey", apiKey.ToString());

            Ships.Log($"Notice.VanguardNotice/ request.Headers: {request.Headers}");

            request.Content = new StringContent(
                payload,
                Encoding.UTF8,
                "application/json"
            );

            Ships.Log($"Notice.VanguardNotice/ request.Content: {request.Content}");

            return request;
        }
    }
}
