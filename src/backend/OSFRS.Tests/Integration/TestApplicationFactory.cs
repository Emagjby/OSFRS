using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OSFRS.Backend.Data;

namespace OSFRS.Tests.Integration;

public class TestApplicationFactory : WebApplicationFactory<Program>
{
    private static readonly object _lock = new();
    private static bool _initialized = false;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Set JWT vars
        Environment.SetEnvironmentVariable("JWT_SECRET", "Xo8pCrcllE87HPhyaBbR6bo2gN0gh/obKNGBhVb1r1U=");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "TestIssuer");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "TestAudience");

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<OSFRSDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Use shared DB name per test class (based on test type name)
            var dbName = GetType().Name; 
            services.AddDbContext<OSFRSDbContext>(options =>
                options.UseInMemoryDatabase(dbName));

            // Initialize once per factory
            lock (_lock)
            {
                if (!_initialized)
                {
                    using var scope = services.BuildServiceProvider().CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<OSFRSDbContext>();
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();
                    _initialized = true;
                }
            }
        });
    }
}