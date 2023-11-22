//path: src\Utilities\VesselInfo.cs

namespace Neurocache.Utilities
{
    public static class VesselInfo
    {
        public static readonly string FleetName = "neurocache";
        public static readonly string ThisVessel = "dotnet_client_shuttle";

        public static readonly string VanguardName = "dotnet_vanguard_starship";

        public static string VesselAddress(string vessel, int port)
            => $"http://{vessel}/{FleetName}.koyeb:{port}";
    }
}
