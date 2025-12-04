using Microsoft.EntityFrameworkCore;
using OSFRS.Backend.Data;

namespace OSFRS.IntegrationTests.Infrastructure;

public sealed class IntegrationDbContext : IAsyncDisposable
{
    public OSFRSDbContext Db { get; }

    public IntegrationDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<OSFRSDbContext>()
            .UseInMemoryDatabase(databaseName: $"{dbName}")
            .EnableSensitiveDataLogging()
            .Options;

        Db = new OSFRSDbContext(options);

        Db.Database.EnsureCreated();
    }

    public async ValueTask DisposeAsync()
    {
        await Db.Database.EnsureDeletedAsync();
        await Db.DisposeAsync();
    }
}
