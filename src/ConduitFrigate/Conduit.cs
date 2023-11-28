//path: src\ConduitFrigate\Conduit.cs

using System.Reactive.Disposables;
using Confluent.Kafka.Admin;
using System.Reactive.Linq;
using Confluent.Kafka;
using System.Reactive;

using Neurocache.ShipsInfo;
using Neurocache.Schema;

namespace Neurocache.ConduitFrigate
{
    public static class Conduit
    {
        private static readonly ProducerConfig uplinkConfig = CreateProducer();
        private static readonly ConsumerConfig downlinkConfig = CreateConsumer();

        public static IConsumer<string, OperationReport> DownlinkConsumer
        => new ConsumerBuilder<string, OperationReport>(downlinkConfig)
            .SetValueDeserializer(new JsonOperationReportDeserializer())
            .Build();

        public static IProducer<string, OperationReport> UplinkProducer
            => new ProducerBuilder<string, OperationReport>(uplinkConfig)
                .SetValueSerializer(new JsonOperationReportSerializer())
                .Build();

        public static async Task EnsureTopicExists(Guid topic)
            => await CreateTopicIfNotExist(uplinkConfig, topic.ToString());

        public static IObservable<OperationReport> Downlink(string topic, IConsumer<string, OperationReport> downlink, CancellationToken cancelToken)
        {
            downlink.Subscribe(topic);
            return Observable.Interval(TimeSpan.FromSeconds(0.1))
                .Select(_ => downlink.Consume(cancelToken).Message.Value)
                .Where(message => message != null)
                .TakeUntil(Observable.Create<Unit>(observer =>
                {
                    cancelToken.Register(() => observer.OnCompleted());
                    return Disposable.Empty;
                }));
        }

        public static async void Uplink(string topic, OperationReport operationReport, CancellationToken cancelToken)
        {
            using var uplink = new ProducerBuilder<string, OperationReport>(uplinkConfig).Build();
            await uplink.ProduceAsync(topic, new Message<string, OperationReport>
            {
                Key = operationReport.Token.ToString(),
                Value = operationReport
            }, cancelToken);
        }

        static ProducerConfig CreateProducer()
        {
            var bootstrapServers = Environment.GetEnvironmentVariable("BOOTSTRAP_SERVERS");
            var sasslUsername = Environment.GetEnvironmentVariable("SASL_USERNAME");
            var sasslPassword = Environment.GetEnvironmentVariable("SASL_PASSWORD");
            return new()
            {
                BootstrapServers = bootstrapServers,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = sasslUsername,
                SaslPassword = sasslPassword
            };
        }

        static ConsumerConfig CreateConsumer()
        {
            var bootstrapServers = Environment.GetEnvironmentVariable("BOOTSTRAP_SERVERS");
            var sasslUsername = Environment.GetEnvironmentVariable("SASL_USERNAME");
            var sasslPassword = Environment.GetEnvironmentVariable("SASL_PASSWORD");

            return new()
            {
                BootstrapServers = bootstrapServers,
                GroupId = Ships.ThisVesselName,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = sasslUsername,
                SaslPassword = sasslPassword
            };
        }

        static async Task CreateTopicIfNotExist(ProducerConfig config, string topic)
        {
            using var adminClient = new AdminClientBuilder(config).Build();
            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(30));
            if (!metadata.Topics.Any(t => t.Topic == topic))
            {
                var topicSpecification = new TopicSpecification
                {
                    Name = topic,
                    NumPartitions = 1,
                    ReplicationFactor = 3
                };

                await adminClient.CreateTopicsAsync(new[] { topicSpecification });
            }
        }
    }
}
