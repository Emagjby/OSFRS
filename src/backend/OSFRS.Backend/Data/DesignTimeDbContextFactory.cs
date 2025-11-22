using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OSFRS.Backend.Data;

/// <summary>
/// Provides a design-time factory for creating <see cref="OSFRSDbContext"/> instances.
/// Used by Entity Framework Core tools (migrations, scaffolding) when the app is not running.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<OSFRSDbContext>
{
    /// <summary>
    /// Creates a new <see cref="OSFRSDbContext"/> using the connection string
    /// stored in the OSFRS_DB_CONN environment variable.
    /// </summary>
    /// <param name="args">Command-line arguments passed by EF tooling.</param>
    /// <returns>A configured <see cref="OSFRSDbContext"/> instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the required environment variable OSFRS_DB_CONN is not set.
    /// </exception>
    public OSFRSDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OSFRSDbContext>();

        var conn = Environment.GetEnvironmentVariable("OSFRS_DB_CONN");
        if (string.IsNullOrEmpty(conn))
            throw new InvalidOperationException("Environment variable OSFRS_DB_CONN not set.");

        optionsBuilder.UseNpgsql(conn);
        return new OSFRSDbContext(optionsBuilder.Options);
    }
}