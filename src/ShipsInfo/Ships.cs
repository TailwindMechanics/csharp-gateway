//path: src\ShipsInfo\Ships.cs

namespace Neurocache.ShipsInfo
{
    public static class Ships
    {
        public static void Log(string message)
            => Serilog.Log.Information($"==> {message}");

        public static readonly string FleetName = "neurocache_fleet";
        public static readonly string ThisVessel = "dotnet_client_shuttle";

        public static string VanguardAddress()
            => VesselAddress("dotnet_vanguard_starship", 5001);

        public static readonly List<string> Cruisers = [
            "dotnet_cruiser",
        ];

        public static string VesselAddress(string vessel, int port)
            => $"http://{vessel}.neurocache.koyeb:{port}"
                .Replace("_", "-");
    }
}
