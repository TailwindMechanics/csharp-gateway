// path: /Program.cs

using dotenv.net;
using Serilog;

using Neurocache.ConduitFrigate;
using Neurocache.LogbookFrigate;
using Neurocache.Lifetime;


var builder = WebApplication.CreateBuilder(args);
{
    DotEnv.Fluent().WithEnvFiles().WithOverwriteExistingVars()
        .WithProbeForEnv(probeLevelsToSearch: 6).Load();

    var port = Environment.GetEnvironmentVariable("PORT");
    builder.WebHost.UseUrls($"http://*:{port}");
    builder.Services.AddControllers();
    builder.Services.AddSingleton(Conduit.DownlinkConfig);
}

var app = builder.Build();
{
    Log.Logger = Logbook.CreateLogger();
    app.MapControllers();
    new Lifetime().Subscribe(app.Services);
    app.Run();
}
