using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddOcelot(builder.Configuration.GetSection("Ocelot"));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapGet("/", () => "Gateway up");
app.MapGet("/swagger", () => Results.Redirect("/swagger/index.html"));

await app.UseOcelot();

app.Run();