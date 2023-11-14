
var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddControllers();
}

var app = builder.Build();
{
    app.UseHttpsRedirection();
    app.MapControllers();
    Console.WriteLine("<--- Neurocache Gateway Started --->");
    app.Run();
}
