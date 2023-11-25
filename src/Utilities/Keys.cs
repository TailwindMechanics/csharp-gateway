//path: src\Utilities\Keys.cs

using Neurocache.ShipsInfo;

namespace Neurocache.Utilities
{
    public static class Keys
    {
        public static bool Guard(HttpRequest request)
            => Guard(request, out _);

        public static bool Guard(HttpRequest request, out Guid apiKey)
        {
            if (!Guid.TryParse(request.Headers["apikey"], out apiKey))
            {
                Ships.Log("Unauthorized: Invalid API Key format");
                return false;
            }

            return true;
        }
    }
}
