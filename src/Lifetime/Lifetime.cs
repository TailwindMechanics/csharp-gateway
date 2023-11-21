//path: src\Lifetime\Lifetime.cs

using Serilog;

using Neurocache.Utilities;

namespace Neurocache.Lifetime
{
    public class Lifetime()
    {
        public void Subscribe(IServiceProvider serviceProvider, Action? onStarted = null, Action? onEnded = null)
        {
            var lifetime = serviceProvider.GetRequiredService<IHostApplicationLifetime>();
            lifetime.ApplicationStarted.Register(() =>
            {
                onStarted?.Invoke();
                Log.Information($"<--- {VesselInfo.ThisVessel}: Online --->");
            });
            lifetime.ApplicationStopping.Register(() =>
            {
                onEnded?.Invoke();
                Log.Information($"<--- {VesselInfo.ThisVessel}: Offline --->");
                Log.CloseAndFlush();
            });
        }
    }
}
