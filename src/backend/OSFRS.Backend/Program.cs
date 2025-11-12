using OSFRS.Backend.Data;
using OSFRS.Backend.Repositories;
using OSFRS.Backend.Services;
using OSFRS.Backend.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using DotNetEnv;
using System.Text;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Helpers.Logging;
using OSFRS.Models.Entities;
using OSFRS.Backend.Services.Background;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

if (!builder.Environment.EnvironmentName.Equals("Testing", StringComparison.OrdinalIgnoreCase))
{
    var connString = Environment.GetEnvironmentVariable("OSFRS_DB_CONN");
    if (string.IsNullOrWhiteSpace(connString))
        throw new Exception("Database connection string not found in envvars");

    builder.Services.AddDbContext<OSFRSDbContext>(options =>
        options.UseNpgsql(connString)); 
}

// Dependency Injection
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IFacilityRepository, FacilityRepository>();
builder.Services.AddScoped<IFacilityService, FacilityService>();
builder.Services.AddScoped<IMaintenanceRepository, MaintenanceRepository>();
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();
builder.Services.AddScoped(typeof(IAppLogger<>), typeof(AppLogger<>));

// Hosted services
builder.Services.AddHostedService<FacilityStatusSyncService>();

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