//path: src\ConduitFrigate\Serializers.cs

using Confluent.Kafka;
using Newtonsoft.Json;
using System.Text;

using Neurocache.Schema;

namespace Neurocache.ConduitFrigate
{
    public class JsonOperationReportSerializer : ISerializer<OperationReport>
    {
        public byte[] Serialize(OperationReport data, SerializationContext context)
            => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
    }

    public class JsonOperationReportDeserializer : IDeserializer<OperationReport>
    {
        public OperationReport Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
            => JsonConvert.DeserializeObject<OperationReport>(Encoding.UTF8.GetString(data.ToArray()))!;
    }
}