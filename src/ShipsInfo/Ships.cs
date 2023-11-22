//path: src\ShipsInfo\Ships.cs

namespace Neurocache.ShipsInfo
{
    public static class Ships
    {
        public static void Log(string message)
            => Serilog.Log.Information($"---> {ThisVessel}: {message}");

        public static readonly string FleetName = "neurocache_fleet";
        public static readonly string ThisVessel = "dotnet_client_shuttle";

        public static readonly string Vanguard = "dotnet_vanguard_starship";

        public static readonly Dictionary<string, int> Cruisers = new()
        {
            { "dotnet_cruiser", 5050 },
        };

        public static string VesselAddress(string vessel, int port)
            => $"http://{vessel}/{FleetName}.koyeb:{port}";
    }
}
