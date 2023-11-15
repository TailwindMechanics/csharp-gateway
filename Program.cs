// path: /Program.cs

using KafkaFlow.Configuration;
using KafkaFlow.Serializer;
using dotenv.net;
using KafkaFlow;
using Supabase;
using Serilog;

using Neurocache.Gateway.Utilities;

var envVars = DotEnv.Fluent()
    .WithExceptions()
    .WithEnvFiles()
    .WithTrimValues()
    .WithOverwriteExistingVars()
    .WithProbeForEnv(probeLevelsToSearch: 6)
    .Read();

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddControllers();

    builder.Services.AddKafka(kafka =>
    {
        kafka
        .AddCluster(cluster =>
        {
            cluster
            .WithBrokers(new[] { envVars["KAFKA_BOOTSTRAP_SERVERS"] })
            .WithSecurityInformation(info =>
            {
                info.SecurityProtocol = SecurityProtocol.SaslSsl;
                info.SaslMechanism = SaslMechanism.ScramSha256;
                info.SaslUsername = envVars["KAFKA_SASL_USERNAME"];
                info.SaslPassword = envVars["KAFKA_SASL_PASSWORD"];
            })
            .CreateTopicIfNotExists(KafkaUtils.TopicName, 1, 1)
            .AddProducer(
                KafkaUtils.ProducerName,
                producer =>
                {
                    producer.DefaultTopic(KafkaUtils.TopicName);
                    producer.AddMiddlewares(middlewares =>
                    {
                        middlewares.AddSerializer<NewtonsoftJsonSerializer>();
                    });
                });
        });
    });

    builder.Services.AddScoped(_ =>
    {
        return new Client
        (
            envVars["SUPABASE_URL"],
            envVars["SUPABASE_KEY"],
            new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            }
        );
    });
}

Log.Logger = new LoggerConfiguration()
    .WriteTo.BetterStack(sourceToken: envVars["BETTERSTACK_LOGS_SOURCE_TOKEN"])
    .WriteTo.Console()
    .CreateLogger();

var app = builder.Build();
{
    app.UseHttpsRedirection();
    app.MapControllers();

    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
    lifetime.ApplicationStarted.Register(() =>
    {
        Log.Information("<--- Neurocache Gateway Started --->");
    });
    lifetime.ApplicationStopping.Register(() =>
    {
        Log.Information("<--- Neurocache Gateway Stopped --->");
        Log.CloseAndFlush();
    });

    app.Run();
}
