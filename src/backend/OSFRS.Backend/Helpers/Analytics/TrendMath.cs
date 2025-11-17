namespace OSFRS.Backend.Helpers.Analytics;

public static class TrendMath
{
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

    public static double GrowthRate(IEnumerable<int> values)
    {
        var list = values.ToList();
        if (list.Count() < 2 || list.First() == 0)
            return 0;

        return ((double)list.Last() - list.First()) / list.First() * 100;
    }
}