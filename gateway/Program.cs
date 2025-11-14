var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();                       // <- UI
app.MapControllers();
app.MapGet("/", () => "Gateway up");     // <- health

// ako /swagger (bez index.html) i dalje daje 404, dodaj ovaj redirect:
app.MapGet("/swagger", () => Results.Redirect("/swagger/index.html"));

app.Run();
