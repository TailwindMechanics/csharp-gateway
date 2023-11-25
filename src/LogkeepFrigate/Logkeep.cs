//path: src\LogkeepFrigate\Logkeep.cs

using Serilog.Sinks.Elasticsearch;
using Serilog.Core;
using Serilog;

using Neurocache.ShipsInfo;

namespace Neurocache.LogkeepFrigate
{
    public static class Logkeep
    {
        public static Logger SystemLogger()
            => CreateLogger("system_logs", $"{Ships.FleetName}", "SYSTEM_LOGGING_LEVEL");

        public static Logger ShipLogger()
        {
            Serilog.Debugging.SelfLog.Enable(Console.Error);
            return CreateLogger("ships_log", $"{Ships.FleetName}", "SHIP_LOGGING_LEVEL");
        }

        static Logger CreateLogger(string category, string index, string envarKey)
        {
            var level = Environment.GetEnvironmentVariable(envarKey);
            var serilogLevel = level switch
            {
                "Verbose" => Serilog.Events.LogEventLevel.Verbose,
                "Debug" => Serilog.Events.LogEventLevel.Debug,
                "Information" => Serilog.Events.LogEventLevel.Information,
                "Warning" => Serilog.Events.LogEventLevel.Warning,
                "Error" => Serilog.Events.LogEventLevel.Error,
                "Fatal" => Serilog.Events.LogEventLevel.Fatal,
                _ => Serilog.Events.LogEventLevel.Information,
            };

            Serilog.Debugging.SelfLog.Enable(Console.Error);

            var username = Environment.GetEnvironmentVariable("ELASTIC_USERNAME")!;
            var password = Environment.GetEnvironmentVariable("ELASTIC_PASSWORD")!;
            var uri = Environment.GetEnvironmentVariable("ELASTIC_URI")!;

            return new LoggerConfiguration()
                .MinimumLevel.Is(serilogLevel)
                .Enrich.WithProperty("Category", category)
                .Enrich.WithProperty("Ship", Ships.ThisVessel)
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
