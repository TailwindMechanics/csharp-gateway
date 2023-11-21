//path: src\LogbookFrigate\Logbook.cs

using Serilog.Core;
using Serilog;

namespace Neurocache.LogbookFrigate
{
    public static class Logbook
    {
        public static Logger CreateLogger(IDictionary<string, string> envVars)
            => new LoggerConfiguration()
                .WriteTo.BetterStack(sourceToken: envVars["BETTERSTACK_LOGS_SOURCE_TOKEN"])
                .WriteTo.Console()
                .CreateLogger();
    }
}