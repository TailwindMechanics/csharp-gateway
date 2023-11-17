// path: /Program.cs

using dotenv.net;
using Supabase;
using Serilog;

var envVars = DotEnv.Fluent()
    .WithExceptions()
    .WithEnvFiles()
    .WithTrimValues()
    .WithOverwriteExistingVars()
    .WithProbeForEnv(probeLevelsToSearch: 6)
    .Read();

var builder = WebApplication.CreateBuilder(args);
{
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
    app.UseHttpsRedirection();
    app.MapControllers();

    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
    lifetime.ApplicationStarted.Register(() =>
    {
        Log.Information("<--- Gateway Started --->");
    });
    lifetime.ApplicationStopping.Register(() =>
    {
        Log.Information("<--- Gateway Stopped --->");
        Log.CloseAndFlush();
    });

    app.Run();
}
