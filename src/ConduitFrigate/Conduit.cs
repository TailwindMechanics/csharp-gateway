//path: src\ConduitFrigate\Conduit.cs

using System.Reactive.Disposables;
using Confluent.Kafka.Admin;
using System.Reactive.Linq;
using Confluent.Kafka;
using System.Reactive;

using Neurocache.Utilities;
using Neurocache.Schema;

namespace Neurocache.ConduitFrigate
{
    public static class Conduit
    {
        public static ProducerConfig UplinkConfig
            => CreateProducer();
        public static ConsumerConfig DownlinkConfig
            => CreateConsumer();

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

        public static async void Uplink(string topic, IProducer<string, OperationReport> uplink, OperationReport operationReport, CancellationToken cancelToken)
        {
            await CreateTopicIfNotExist(UplinkConfig, topic);
            await uplink.ProduceAsync(topic, new Message<string, OperationReport>
            {
                Key = operationReport.Token,
                Value = operationReport
            }, cancelToken);
        }

        static ProducerConfig CreateProducer()
        {
            var bootstrapServers = Environment.GetEnvironmentVariable("BOTTSTRAP_SERVERS");
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
            var bootstrapServers = Environment.GetEnvironmentVariable("BOTTSTRAP_SERVERS");
            var sasslUsername = Environment.GetEnvironmentVariable("SASL_USERNAME");
            var sasslPassword = Environment.GetEnvironmentVariable("SASL_PASSWORD");

            return new()
            {
                BootstrapServers = bootstrapServers,
                GroupId = VesselInfo.ThisVessel,
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
                    ReplicationFactor = 1
                };

                await adminClient.CreateTopicsAsync(new[] { topicSpecification });
            }
        }
    }
}