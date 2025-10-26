using OSFRS.Backend.Data;
using OSFRS.Backend.Repositories;
using OSFRS.Backend.Services;
using OSFRS.Backend.Helpers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

builder.Services.AddDbContext<OSFRSDbContext>(options =>
    options.UseNpgsql(Environment.GetEnvironmentVariable("OSFRS_DB_CONN")));

// Dependency Injection
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<PasswordHasher>();

// Controllers
builder.Services.AddControllers();

var app = builder.Build();

app.UseRouting();

app.MapControllers();

app.MapGet("/", () => "API is running!");

app.Run();