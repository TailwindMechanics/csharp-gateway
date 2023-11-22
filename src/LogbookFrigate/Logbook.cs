//path: src\LogbookFrigate\Logbook.cs

using Serilog.Core;
using Serilog;

using Neurocache.Utilities;

namespace Neurocache.LogbookFrigate
{
    public static class Logbook
    {
        public static Logger CreateLogger()
        {
            var enviornment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var apiKey = Environment.GetEnvironmentVariable("DATADOG_API_KEY");

            return new LoggerConfiguration()
                .WriteTo.DatadogLogs(
                    apiKey: apiKey,
                    source: VesselInfo.FleetName,
                    service: VesselInfo.ThisVessel,
                    host: enviornment,
                    tags: [$"env:{enviornment}"]
                )
                .WriteTo.Console()
                .CreateLogger();
        }
    }
}
