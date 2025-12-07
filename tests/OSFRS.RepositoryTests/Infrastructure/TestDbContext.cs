using OSFRS.Backend.Data;

namespace OSFRS.RepositoryTests.Infrastructure;

public class TestDbContext : IAsyncDisposable
{
    public OSFRSDbContext Db { get; }

    public TestDbContext()
    {
        Db = DbContextFactory.Create();
        Db.Database.EnsureCreated();
    }

    public async ValueTask DisposeAsync()
    {
        await Db.Database.EnsureDeletedAsync();
        await Db.DisposeAsync();
    }
}
