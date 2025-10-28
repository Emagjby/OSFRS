using OSFRS.Backend.Data;
using OSFRS.Backend.Repositories;
using OSFRS.Backend.Services;
using OSFRS.Backend.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using DotNetEnv;
using System.Text;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Helpers.Logging;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

if (!builder.Environment.EnvironmentName.Equals("Testing", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddDbContext<OSFRSDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Dependency Injection
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProfileService>();
builder.Services.AddScoped<PasswordHasher>();
builder.Services.AddScoped<JwtTokenGenerator>();
builder.Services.AddScoped(typeof(IAppLogger<>), typeof(AppLogger<>));

// Controllers
builder.Services.AddControllers();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
            ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")!)
            )
        };
    });

var app = builder.Build();

app.UseRouting();

// Add JWT auth middleware
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Debug
app.MapGet("/", () => "API is running!");

app.Run();

public partial class Program { }