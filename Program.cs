// path: /Program.cs

using dotenv.net;
using Supabase;
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
        { "SUPABASE_URL", Environment.GetEnvironmentVariable("SUPABASE_URL")! },
        { "SUPABASE_KEY", Environment.GetEnvironmentVariable("SUPABASE_KEY")! },
        { "BETTERSTACK_LOGS_SOURCE_TOKEN", Environment.GetEnvironmentVariable("BETTERSTACK_LOGS_SOURCE_TOKEN")! }
    };
}

var builder = WebApplication.CreateBuilder(args);
{
    var port = Environment.GetEnvironmentVariable("PORT");
    builder.WebHost.UseUrls($"http://*:{port}");

    builder.Services.AddControllers();
    builder.Services.AddScoped(_ =>
    {
        return new Client
        (
            envVars["SUPABASE_URL"],
            envVars["SUPABASE_KEY"],
            new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            }
        );
    });
}

Log.Logger = new LoggerConfiguration()
    .WriteTo.BetterStack(sourceToken: envVars["BETTERSTACK_LOGS_SOURCE_TOKEN"])
    .WriteTo.Console()
    .CreateLogger();

var app = builder.Build();
{
    app.MapControllers();

    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
    lifetime.ApplicationStarted.Register(() =>
    {
        Log.Information("<--- Csharp Gateway Started --->");
    });
    lifetime.ApplicationStopping.Register(() =>
    {
        Log.Information("<--- Csharp Gateway Stopped --->");
        Log.CloseAndFlush();
    });

    app.Run();
}
