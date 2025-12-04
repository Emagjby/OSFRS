namespace OSFRS.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected IntegrationDbContext DbContext { get; private set; } = default!;
    private readonly string _dbName;

    public ServiceFactory Factory { get; private set; } = default!;

    protected IntegrationTestBase(string dbName)
    {
        _dbName = dbName;
    }

    public virtual Task InitializeAsync()
    {
        Environment.SetEnvironmentVariable(
            "JWT_SECRET",
            "biq98nfFZLDDdlcm+7kt49PNDy4GSSmaK7aHdm8dCA4="
        );
        Environment.SetEnvironmentVariable("JWT_ISSUER", "OSFRS.TestIssuer");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "OSFRS.TestAudience");
        Environment.SetEnvironmentVariable("JWT_EXPIRY_MINUTES", "60");

        DbContext = new IntegrationDbContext(_dbName);
        Factory = new ServiceFactory(DbContext.Db);

        TestClock.Reset();

        return Task.CompletedTask;
    }

    public virtual async Task DisposeAsync()
    {
        TestClock.Reset();

        if (DbContext is not null)
            await DbContext.DisposeAsync();
    }

    protected IDisposable FreezeAt(DateTime utcNow) => TestClock.Freeze(utcNow);
}
