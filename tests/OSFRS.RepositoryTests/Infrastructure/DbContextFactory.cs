using Microsoft.EntityFrameworkCore;
using OSFRS.Backend.Data;

namespace OSFRS.RepositoryTests.Infrastructure;

public static class DbContextFactory
{
    public static OSFRSDbContext Create(string? name = null)
    {
        var options = new DbContextOptionsBuilder<OSFRSDbContext>()
            .UseInMemoryDatabase(databaseName: name ?? Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        return new OSFRSDbContext(options);
    }
}
