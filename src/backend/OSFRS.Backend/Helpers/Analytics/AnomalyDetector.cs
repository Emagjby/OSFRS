namespace OSFRS.Backend.Helpers.Analytics;

public static class AnomalyDetector
{
    public static List<int> DetectByZScore(IEnumerable<int> values, double threshold = 3.0)
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

    private static double MedianInt(List<int> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        int mid = sorted.Count / 2;

        return sorted.Count % 2 == 0
            ? (sorted[mid - 1] + sorted[mid]) / 2.0
            : sorted[mid];
    }

    private static double MedianDouble(List<double> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        int mid = sorted.Count / 2;

        return sorted.Count % 2 == 0
            ? (sorted[mid - 1] + sorted[mid]) / 2.0
            : sorted[mid];
    }
}