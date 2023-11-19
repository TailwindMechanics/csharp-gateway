//path: src\Utilities\Keys.cs

using Serilog;

namespace Neurocache.Utilities
{
    public static class Keys
    {
        public static bool Guard(HttpRequest request, out Guid apiKey)
        {
            if (!Guid.TryParse(request.Headers["apikey"], out apiKey))
            {
                Log.Information("Unauthorized: Invalid API Key format");
                return false;
            }

            return true;
        }
    }
}
