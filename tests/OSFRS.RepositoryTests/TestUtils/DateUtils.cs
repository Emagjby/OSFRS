namespace OSFRS.RepositoryTests.TestUtils;

public static class DateUtils
{
    private static DateTime? _frozenUtcNow;

    public static DateTime UtcNow => _frozenUtcNow ?? DateTime.UtcNow;

    public static void Freeze(DateTime utcNow)
    {
        _frozenUtcNow = utcNow;
    }

    public static void Unfreeze()
    {
        _frozenUtcNow = null;
    }

    public static IDisposable FreezeScope(DateTime utcNow) => new FreezeContext(utcNow);

    private sealed class FreezeContext : IDisposable
    {
        public FreezeContext(DateTime time)
        {
            Freeze(time);
        }

        public void Dispose()
        {
            Unfreeze();
        }
    }

    public static DateTime At(
        int year,
        int month,
        int day,
        int hour = 0,
        int minute = 0,
        int second = 0
    ) => new(year, month, day, hour, minute, second, DateTimeKind.Utc);

    public static DateTime Offset(DateTime from, int days = 0, int hours = 0, int minutes = 0) =>
        from.AddDays(days).AddHours(hours).AddMinutes(minutes);

    public static DateTime UtcNowTrim =>
        new DateTime(
            UtcNow.Year,
            UtcNow.Month,
            UtcNow.Day,
            UtcNow.Hour,
            UtcNow.Minute,
            UtcNow.Second,
            DateTimeKind.Utc
        );
}
