//path: src\Schema\OperationReport.cs

using Newtonsoft.Json;

namespace Neurocache.Schema
{
    public class OperationReport(
        string token,
        string author,
        string payload,
        bool final,
        List<string> dependents
    )
    {
        public string Token { get; } = token;
        public string Author { get; } = author;
        public string Payload { get; } = payload;
        public bool Final { get; } = final;
        public List<string> Dependents { get; } = dependents;

        public override string ToString()
        {
            var dependentsStr = Dependents != null ? string.Join(", ", Dependents) : "None";
            return $"Token: {Token}, Author: {Author}, Payload: {Payload}, Final: {Final}, Dependents: {dependentsStr}";
        }

        public static OperationReport? FromJson(string json)
            => JsonConvert.DeserializeObject<OperationReport>(json);
    }
}
