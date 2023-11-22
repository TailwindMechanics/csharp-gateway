//path: src\Lifetime\Lifetime.cs

using Serilog;

using Neurocache.ShipsInfo;

namespace Neurocache.Lifetime
{
    public class Lifetime()
    {
        public void Subscribe(IServiceProvider serviceProvider, Action? onStarted = null, Action? onEnded = null)
        {
            var lifetime = serviceProvider.GetRequiredService<IHostApplicationLifetime>();
            lifetime.ApplicationStarted.Register(() =>
            {
                Ships.Log("Online");
                onStarted?.Invoke();
            });
            lifetime.ApplicationStopping.Register(() =>
            {
                Ships.Log("Offline");
                Log.CloseAndFlush();
                onEnded?.Invoke();
            });
        }
    }
}
