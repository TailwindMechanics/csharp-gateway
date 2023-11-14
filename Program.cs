// path: /Program.cs

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddControllers();
}

var app = builder.Build();
{
    app.UseHttpsRedirection();
    app.MapControllers();

    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
    lifetime.ApplicationStarted.Register(() =>
    {
        Console.WriteLine("<--- Neurocache Gateway Started --->");
    });
    lifetime.ApplicationStopping.Register(() =>
    {
        Console.WriteLine("<--- Neurocache Gateway Stopped --->");
    });

    app.Run();
}
