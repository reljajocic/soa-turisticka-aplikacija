using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Tours.API.Application.Interfaces;
using Tours.API.Application.Services;
using Tours.API.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// Mongo context + services
builder.Services.AddSingleton<TourMongoContext>();
builder.Services.AddScoped<ITourService, TourService>();
builder.Services.AddScoped<IPositionService, PositionService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IExecutionService, ExecutionService>();

// JWT
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Jwt:Secret"]!));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(o => o.TokenValidationParameters = new()
  {
      ValidIssuer = cfg["Jwt:Issuer"],
      ValidAudience = cfg["Jwt:Audience"],
      IssuerSigningKey = key,
      ValidateIssuer = true,
      ValidateAudience = true,
      ValidateIssuerSigningKey = true,
      ValidateLifetime = true
  });
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
