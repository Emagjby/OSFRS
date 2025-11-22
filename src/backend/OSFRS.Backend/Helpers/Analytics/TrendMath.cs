namespace OSFRS.Backend.Helpers.Analytics;

/// <summary>
/// Provides mathematical helpers used for trend analysis across time-series
/// usage data. Includes moving averages, percentage deltas, and overall
/// growth rate computations.
/// </summary>
public static class TrendMath
{
    /// <summary>
    /// Computes a moving average over a sequence of integer values using
    /// the specified window size.
    ///
    /// If the window size is invalid (≤ 1 or larger than the dataset),
    /// the method returns the original values converted to <see cref="double"/>.
    /// </summary>
    /// <param name="values">The numeric time-series values.</param>
    /// <param name="windowSize">The number of points included in each averaged window.</param>
    /// <returns>
    /// A list of moving average values aligned with the original index positions.
    /// </returns>
    public static List<double> MovingAverage(IEnumerable<int> values, int windowSize)
    {
        var list = values.ToList();

        if (windowSize <= 1 || windowSize > list.Count())
            return list.Select(v => (double)v).ToList();

        List<double> result = new();

        for (int i = 0; i < list.Count(); i++)
        {
            int start = Math.Max(0, i - windowSize + 1);
            var slice = list.Skip(start).Take(windowSize);
            result.Add(slice.Average());
        }

        return result;
    }

    /// <summary>
    /// Computes the percentage change between sequential values in a time-series.
    ///
    /// If a previous value is zero, the change is recorded as <c>0</c> to avoid
    /// division-by-zero errors.
    /// </summary>
    /// <param name="values">The numeric time-series values.</param>
    /// <returns>
    /// A list of percentage change values, where index <c>i</c> represents the
    /// change from <c>values[i - 1]</c> to <c>values[i]</c>.
    /// </returns>
    public static List<double> PercentageChanges(IEnumerable<int> values)
    {
        var list = values.ToList();
        List<double> changes = new();

        for (int i = 1; i < list.Count(); i++)
        {
            if (list[i - 1] == 0)
                changes.Add(0);
            else
                changes.Add(((double)list[i] - list[i - 1]) / list[i - 1] * 100);
        }

        return changes;
    }

    /// <summary>
    /// Computes the overall growth rate from the first value in the series
    /// to the last value, expressed as a percentage.
    ///
    /// If the dataset contains fewer than two points or begins with zero,
    /// the growth rate is returned as <c>0</c>.
    /// </summary>
    /// <param name="values">The numeric time-series values.</param>
    /// <returns>
    /// The percentage growth from the first to the last value.
    /// </returns>
    public static double GrowthRate(IEnumerable<int> values)
    {
        var list = values.ToList();
        if (list.Count() < 2 || list.First() == 0)
            return 0;

        return ((double)list.Last() - list.First()) / list.First() * 100;
    }
}