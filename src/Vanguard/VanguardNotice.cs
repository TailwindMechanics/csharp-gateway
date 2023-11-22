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
            var request = VanguardNotice(
                JsonConvert.SerializeObject(operationRequest),
                "operation/request",
                apiKey
            );

            var response = await VanguardResponse(request);
            var content = await response.Content.ReadAsStringAsync();
            var operationToken = JsonConvert.DeserializeObject<OperationToken>(content);

            return operationToken;
        }

        static async Task<HttpResponseMessage> VanguardResponse(HttpRequestMessage request)
        {
            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode) Log.Warning($"{Ships.Vanguard}: Denied");
            else Log.Information($"{Ships.Vanguard}: Approved");
            return response;
        }

        static HttpRequestMessage VanguardNotice(string payload, string endpoint, Guid apiKey)
        {
            endpoint = endpoint.StartsWith("/") ? endpoint[1..] : endpoint;

            var vanguard = Environment.GetEnvironmentVariable("VANGUARD_ADDRESS");
            var vanguardUrl = $"{vanguard}/{endpoint}";

            var request = new HttpRequestMessage(HttpMethod.Post, vanguardUrl);
            request.Headers.Add("apikey", apiKey.ToString());
            request.Content = new StringContent(
                payload,
                Encoding.UTF8,
                "application/json"
            );

            return request;
        }
    }
}
