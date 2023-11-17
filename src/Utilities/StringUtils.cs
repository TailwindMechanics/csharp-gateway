//path: src\Utilities\StringUtils.cs

using Serilog;

namespace Neurocache.Gateway.Utilities
{
    public static class StringUtils
    {
        public static string NexusRoute(string nexusName, string endpoint)
        => $"http://{nexusName}-nexus.neurocache.internal/{endpoint}";

        public static string StartMessage(this string id, string prompt)
        {
            var message = $"<start [{id}] [{prompt}]>";
            Log.Information(message);
            return message;
        }

        public static string StreamMessage(this string id, string outputLevel)
        {
            var message = $"<emit [{id}], [{outputLevel}]>";
            Log.Information(message);
            return message;
        }

        public static string StopMessage(this string id)
        {
            var message = $"<stop [{id}]>";
            Log.Information(message);
            return message;
        }
    }
}
