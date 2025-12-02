namespace OSFRS.Backend.Helpers.Analytics;

/// <summary>
/// Provides statistical anomaly-detection utilities used by analytics services.
/// Supports Z-Score and MAD (Median Absolute Deviation) based outlier detection.
/// </summary>
public static class AnomalyDetector
{
    /// <summary>
    /// Detects anomalies in a sequence of integer values using the Z-Score method.
    /// </summary>
    /// <param name="values">The numeric series to evaluate.</param>
    /// <param name="threshold">
    /// The minimum Z-score at which a value is considered an anomaly.
    /// Defaults to <c>3.0</c>.
    /// </param>
    /// <returns>
    /// A list of zero-based indices representing points classified as anomalies.
    /// Returns an empty list if input contains fewer than 3 values
    /// or if the standard deviation is zero.
    /// </returns>
    public static List<int> DetectByZScore(IEnumerable<int> values, double threshold = 3)
    {
        var list = values.ToList();
        if (list.Count < 3) return new();

        double avg = list.Average();
        double std = Math.Sqrt(list.Sum(v => Math.Pow(v - avg, 2)) / list.Count);

        if (std == 0) return new();

        List<int> anomalies = new();
        for (int i = 0; i < list.Count; i++)
        {
            double z = Math.Abs((list[i] - avg) / std);
            if (z >= threshold)
                anomalies.Add(i);
        }

        return anomalies;
    }

    /// <summary>
    /// Detects anomalies using the Median Absolute Deviation (MAD) method.
    /// This algorithm is robust against extreme outliers and non-normal distributions.
    /// </summary>
    /// <param name="values">The numeric series to evaluate.</param>
    /// <param name="threshold">
    /// The minimum modified Z-score required to classify a point as an anomaly.
    /// Defaults to <c>3.5</c>.
    /// </param>
    /// <returns>
    /// A list of zero-based indices representing anomalous points.
    /// Returns an empty list for fewer than 3 values or when MAD equals zero.
    /// </returns>
    public static List<int> DetectByMAD(IEnumerable<int> values, double threshold = 3.5)
    {
        var list = values.ToList();
        if (list.Count < 3) return new();

        double median = MedianInt(list);
        var deviations = list.Select(v => Math.Abs(v - median)).ToList();
        double mad = MedianDouble(deviations);

        if (mad == 0) return new();

        List<int> anomalies = new();
        for (int i = 0; i < list.Count; i++)
        {
            double modifiedZ = 0.6745 * Math.Abs(list[i] - median) / mad;
            if (modifiedZ >= threshold)
                anomalies.Add(i);
        }

        return anomalies;
    }

    /// <summary>
    /// Calculates the median of a list of integers.
    /// </summary>
    /// <param name="values">The integer series.</param>
    /// <returns>The median value.</returns>
    private static double MedianInt(List<int> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        int mid = sorted.Count / 2;

        return sorted.Count % 2 == 0
            ? (sorted[mid - 1] + sorted[mid]) / 2.0
            : sorted[mid];
    }

    /// <summary>
    /// Calculates the median of a list of floating-point values.
    /// </summary>
    /// <param name="values">The numeric series.</param>
    /// <returns>The median value.</returns>
    private static double MedianDouble(List<double> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        int mid = sorted.Count / 2;

        return sorted.Count % 2 == 0
            ? (sorted[mid - 1] + sorted[mid]) / 2.0
            : sorted[mid];
    }
}