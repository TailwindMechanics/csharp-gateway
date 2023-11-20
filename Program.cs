// path: /Program.cs

using dotenv.net;
using Neurocache.Utilities;
using Serilog;

IDictionary<string, string>? envVars = null;
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
{
    envVars = DotEnv.Fluent()
        .WithExceptions()
        .WithEnvFiles()
        .WithTrimValues()
        .WithOverwriteExistingVars()
        .WithProbeForEnv(probeLevelsToSearch: 6)
        .Read();
}
else
{
    envVars = new Dictionary<string, string>
    {
        { "BETTERSTACK_LOGS_SOURCE_TOKEN", Environment.GetEnvironmentVariable("BETTERSTACK_LOGS_SOURCE_TOKEN")! }
    };
}

var builder = WebApplication.CreateBuilder(args);
{
    var port = Environment.GetEnvironmentVariable("PORT");
    builder.WebHost.UseUrls($"http://*:{port}");

    builder.Services.AddControllers();
}

var app = builder.Build();
{
    app.MapControllers();

    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
    lifetime.ApplicationStarted.Register(() =>
    {
        Log.Information($"<--- {VesselInfo.ThisVessel}: Online --->");
    });
    lifetime.ApplicationStopping.Register(() =>
    {
        Log.Information($"<--- {VesselInfo.ThisVessel}: Offline --->");
        Log.CloseAndFlush();
    });

    app.Run();
}

Log.Logger = new LoggerConfiguration()
    .WriteTo.BetterStack(sourceToken: envVars["BETTERSTACK_LOGS_SOURCE_TOKEN"])
    .WriteTo.Console()
    .CreateLogger();
