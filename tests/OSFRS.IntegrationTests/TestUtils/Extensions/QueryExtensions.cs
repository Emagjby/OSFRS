namespace OSFRS.IntegrationTests.TestUtils.Extensions;

public static class QueryExtensions
{
    /// <summary>
    /// Verifies that a collection is sorted ascending by a key selector.
    /// </summary>
    public static void ShouldBeSortedAscending<T, TKey>(
        this IEnumerable<T> items,
        Func<T, TKey> keySelector
    )
        where TKey : IComparable<TKey>
    {
        var arr = items.ToList();

        for (int i = 1; i < arr.Count; i++)
        {
            if (keySelector(arr[i]).CompareTo(keySelector(arr[i - 1])) < 0)
                throw new InvalidOperationException("Sequence is not sorted ascending.");
        }
    }

    /// <summary>
    /// Verifies that a collection is sorted descending by a key selector.
    /// </summary>
    public static void ShouldBeSortedDescending<T, TKey>(
        this IEnumerable<T> items,
        Func<T, TKey> keySelector
    )
        where TKey : IComparable<TKey>
    {
        var arr = items.ToList();

        for (int i = 1; i < arr.Count; i++)
        {
            if (keySelector(arr[i]).CompareTo(keySelector(arr[i - 1])) > 0)
                throw new InvalidOperationException("Sequence is not sorted descending.");
        }
    }

    /// <summary>
    /// Returns the dates of a collection — useful in analytics tests.
    /// </summary>
    public static IEnumerable<DateTime> Dates<T>(
        this IEnumerable<T> items,
        Func<T, DateTime> selector
    ) => items.Select(selector);

    /// <summary>
    /// Simple WhereRange extension for timestamps.
    /// </summary>
    public static IEnumerable<T> WhereInRange<T>(
        this IEnumerable<T> source,
        Func<T, DateTime> selector,
        DateTime from,
        DateTime to
    )
    {
        return source.Where(x =>
        {
            var t = selector(x);
            return t >= from && t <= to;
        });
    }
}
