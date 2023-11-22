//path: src\LogkeepFrigate\Logbook.cs

using Serilog.Sinks.Elasticsearch;
using Serilog.Core;
using Serilog;

using Neurocache.ShipsInfo;

namespace Neurocache.LogkeepFrigate
{
    public static class Logkeep
    {
        public static Logger SystemLogger()
            => CreateLogger("system_logs", $"{Ships.FleetName}");

        public static Logger ShipLogger()
        {
            Serilog.Debugging.SelfLog.Enable(Console.Error);
            return CreateLogger("ships_log", $"{Ships.FleetName}");
        }

        static Logger CreateLogger(string category, string index)
        {
            Serilog.Debugging.SelfLog.Enable(Console.Error);

            var username = Environment.GetEnvironmentVariable("ELASTIC_USERNAME")!;
            var password = Environment.GetEnvironmentVariable("ELASTIC_PASSWORD")!;
            var uri = Environment.GetEnvironmentVariable("ELASTIC_URI")!;

            return new LoggerConfiguration()
            .Enrich.WithProperty("Category", category)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(uri))
            {
                AutoRegisterTemplate = true,
                IndexFormat = index,
                ModifyConnectionSettings = x => x.BasicAuthentication(
                    username, password
                ),
            })
            .CreateLogger();
        }
    }
}
