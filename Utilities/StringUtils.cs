
using Serilog;

namespace Neurocache.Gateway.Utilities
{
    public static class StringUtils
    {
        public static string StartMessage(this string id, string prompt)
        {
            var message = $"<start [{id}] [{prompt}]>";
            Log.Information(message);
            return message;
        }

        public static string StreamMessage(this string id, string msg)
        {
            var message = $"<emit [{id}], [{msg}]>";
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
