using OSFRS.Backend.Data;
using OSFRS.Backend.Repositories;
using OSFRS.Backend.Services;
using OSFRS.Backend.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using DotNetEnv;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Helpers.Logging;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.MemoryStorage;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.Backend.Interfaces.Helper;
using OSFRS.Backend.DTOs.Auth;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Backend.Validators.Auth;
using OSFRS.Models.Entities;
using OSFRS.Backend.DTOs.Facilities;
using OSFRS.Backend.Validators.Facilities;
using OSFRS.Backend.Validators.Reservations;
using OSFRS.Backend.DTOs.Reservations;
using OSFRS.Backend.Validators.Usage;
using OSFRS.Backend.Validators.Maintenance;
using OSFRS.Backend.DTOs.Maintenance;
using OSFRS.Backend.Middleware;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using Sprache;
using Microsoft.Extensions.Options;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// DB configuration (skip for tests)
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
builder.Services.AddScoped<IUsageRepository, UsageRepository>();
builder.Services.AddScoped<IUsageService, UsageService>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

// Validators
builder.Services.AddScoped<IValidator<LoginRequestDto>, UserLoginValidator>();
builder.Services.AddScoped<IValidator<UserRegistrationDto>, UserRegistrationValidator>();
builder.Services.AddScoped<IValidator<CreateFacilityDto>, CreateFacilityValidator>();
builder.Services.AddScoped<IValidator<(CreateReservationDto, int)>, CreateReservationValidator>();
builder.Services.AddScoped<IValidator<(UpdateReservationDto dto, Reservation existing, bool isAdmin, int userId)>, UpdateReservationValidator>();
builder.Services.AddScoped<IValidator<(Reservation reservation, int userId)>, CancelReservationValidator>();
builder.Services.AddScoped<IValidator<(Facility facility, bool isAvailable)>, FacilityAvailabilityValidator>();
builder.Services.AddScoped<IValidator<(string? eventType, int? userId, int? facilityId, DateTime? from, DateTime? to)>, UsageQueryValidator>();
builder.Services.AddScoped<IValidator<CreateMaintenanceRecordDto>, CreateMaintenanceValidator>();
builder.Services.AddScoped<IUpdateValidator<UpdatedProfileDto, User>, ProfileUpdateValidator>();
builder.Services.AddScoped<IUpdateValidator<UpdateFacilityDto, Facility>, UpdateFacilityValidator>();
builder.Services.AddScoped<IUpdateValidator<UpdateMaintenanceRecordDto, MaintenanceRecord>, UpdateMaintenanceValidator>();

builder.Services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));

// Hangfire setup
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddHangfire((sp, config) =>
    {
        config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
              .UseSimpleAssemblyNameTypeSerializer()
              .UseRecommendedSerializerSettings()
              .UsePostgreSqlStorage(options =>
              {
                  options.UseNpgsqlConnection(Environment.GetEnvironmentVariable("OSFRS_DB_CONN"));
              });
    });

    // Hangfire server
    builder.Services.AddHangfireServer();
}

// Controllers
builder.Services.AddControllers();

//  JWT Override Hook for Security Tests
builder.Services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();
builder.Services.AddSingleton<IConfigureNamedOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();


builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value!.Errors.Any())
            .Select(x => x.Value!.Errors.First().ErrorMessage)
            .ToArray();

        return new BadRequestObjectResult(errors);
    };
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    options.CustomSchemaIds(type => type.FullName);
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMicroUI", policy =>
    {
        policy.WithOrigins("http://127.0.0.1:8000", "http://localhost:8000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Hangfire jobs
if (!builder.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var jobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

        jobManager.AddOrUpdate<IUsageService>(
            "daily-usage-aggregation",
            service => service.AggregateAsync(),
            "55 23 * * *"
        );

        jobManager.AddOrUpdate<IMaintenanceService>(
            "status-sync",
            service => service.SyncStatusesAsync(),
            "*/1 * * * *"
        );
    }
}

app.UseRouting();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (Environment.GetEnvironmentVariable("DEBUG_MODE") == "Enabled")
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = [new AllowAllHangfireAuthFilter()]
    });

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DocumentTitle = "OSFRS API Docs";
        options.DisplayRequestDuration();
    });
}

app.UseCors("AllowMicroUI");

// JWT Middleware
app.UseAuthentication();
app.UseAuthorization();

// Controllers
app.MapControllers();

// Debug endpoint
app.MapGet("/health", () => "API is running... (v2)");

app.Run();

public partial class Program { }