namespace OSFRS.IntegrationTests.Infrastructure;

public static class TestClock
{
    private static DateTime? _frozenUtcNow;

    public static DateTime UtcNow => _frozenUtcNow ?? DateTime.UtcNow;

    public static IDisposable Freeze(DateTime value)
    {
        _frozenUtcNow = value;
        return new FreezeScope();
    }

    public static void Reset() => _frozenUtcNow = null;

    private sealed class FreezeScope : IDisposable
    {
        public void Dispose() => Reset();
    }
}
