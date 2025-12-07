using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OSFRS.Backend.Data;
using OSFRS.Backend.Helpers.Security;

namespace OSFRS.SecurityTests.Utils;

public class SecurityWebAppFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"OSFRS_Security_{Guid.NewGuid()}";

    public void ResetDatabase()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OSFRSDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
    }

    public Action<TokenValidationParameters>? JwtOverride { get; set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration(
            (ctx, cfg) =>
            {
                Environment.SetEnvironmentVariable(
                    "JWT_SECRET",
                    "Nafg5mh3I2QhJ92hAnW1MVXTUaiKYlAfXrDYgJc5d7k="
                );
                Environment.SetEnvironmentVariable("JWT_ISSUER", "OSFRS.TestIssuer");
                Environment.SetEnvironmentVariable("JWT_AUDIENCE", "OSFRS.TestAudience");
                Environment.SetEnvironmentVariable("JWT_EXPIRY_MINUTES", "60");
            }
        );

        builder.ConfigureServices(services =>
        {
            var hangfireDescriptors = services
            .Where(d => d.ServiceType.FullName != null &&
                        d.ServiceType.FullName.Contains("Hangfire"))
            .ToList();

            foreach (var d in hangfireDescriptors)
                services.Remove(d);

            foreach (
                var d in services
                    .Where(s => s.ServiceType == typeof(JwtValidationOverride))
                    .ToList()
            )
                services.Remove(d);

            foreach (
                var d in services
                    .Where(s => s.ServiceType == typeof(IConfigureOptions<JwtBearerOptions>))
                    .ToList()
            )
                services.Remove(d);

            services.AddSingleton(new JwtValidationOverride());

            services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();

            foreach (
                var db in services
                    .Where(s => s.ServiceType == typeof(DbContextOptions<OSFRSDbContext>))
                    .ToList()
            )
                services.Remove(db);

            services.AddDbContext<OSFRSDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });
        });
    }

    protected override void ConfigureClient(HttpClient client)
    {
        var overrideSvc = Services.GetRequiredService<JwtValidationOverride>();
        overrideSvc.Override = JwtOverride;

        base.ConfigureClient(client);
    }
}
