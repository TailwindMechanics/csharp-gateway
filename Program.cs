// path: /Program.cs

using Serilog;

using Neurocache.ConduitFrigate;
using Neurocache.LogbookFrigate;
using Neurocache.Lifetime;
using Neurocache.Envars;

IDictionary<string, string> envVars = EnvironmentVariables.Init(
    [
        "BETTERSTACK_LOGS_SOURCE_TOKEN"
    ]
);

var builder = WebApplication.CreateBuilder(args);
{
    var port = Environment.GetEnvironmentVariable("PORT");
    builder.WebHost.UseUrls($"http://*:{port}");
    builder.Services.AddControllers();
    builder.Services.AddSingleton(Conduit.DownlinkConfig);
}

var app = builder.Build();
{
    app.MapControllers();
    new Lifetime().Subscribe(app.Services);
    app.Run();
}

Log.Logger = Logbook.CreateLogger(envVars);
