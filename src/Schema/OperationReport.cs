//path: src\Schema\OperationReport.cs

using Newtonsoft.Json;

namespace Neurocache.Schema
{
    public class OperationReport(
        Guid token,
        string author,
        string payload,
        bool final,
        string reportId,
        List<string>? errors = null,
        int status = 200
    )
    {
        public Guid Token { get; } = token;
        public string Author { get; private set; } = author;
        public string Payload { get; } = payload;
        public bool Final { get; } = final;
        public string ReportId { get; } = reportId;

        public List<string>? Errors { get; } = errors;
        public int Status { get; } = status;

        public void SetClientAuthor()
            => Author = "Client";
        public override string ToString()
            => $"OperationReport({Author}, {Payload}, {Token}, {Final}, {ReportId}, {Errors}, {Status})";
        public static OperationReport? FromJson(string json)
            => JsonConvert.DeserializeObject<OperationReport>(json);
        public static string ToJson(OperationReport report)
            => JsonConvert.SerializeObject(report);
    }
}
