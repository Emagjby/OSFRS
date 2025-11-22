using OSFRS.Backend.DTOs.Analytics;
using OSFRS.Backend.DTOs.Reports;
using OSFRS.Backend.Helpers.Analytics;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Service;

namespace OSFRS.Backend.Services;

/// <summary>
/// Provides analytical operations over usage data, including trend computation,
/// anomaly detection, peak usage extraction and visualization dataset building.
/// </summary>
public class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsRepository _repo;
    private readonly IAppLogger<AnalyticsService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyticsService"/> class.
    /// </summary>
    /// <param name="repo">Repository used for analytical data queries.</param>
    /// <param name="logger">Logging abstraction.</param>
    public AnalyticsService(IAnalyticsRepository repo, IAppLogger<AnalyticsService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    /// <summary>
    /// Detects anomalies within a date range, using either Z-Score or MAD (Median Absolute Deviation).
    /// </summary>
    /// <param name="from">Start date (inclusive).</param>
    /// <param name="to">End date (inclusive).</param>
    /// <param name="mode">Detection mode: "z-score" or "mad".</param>
    /// <returns>A report describing detected anomalies.</returns>
    /// <exception cref="ArgumentException">Thrown when an invalid detection mode is specified.</exception>
    public async Task<AnomalyReportDto> DetectAnomaliesAsync(DateTime from, DateTime to, string mode = "z-score")
    {
        var raw = (await _repo.GetDailyCountsAsync(from, to)).ToList();
        var counts = raw.Select(x => x.Count).ToList();

        List<int> anomalyIndexes = mode.ToLower() switch
        {
            "mad" or "median" => AnomalyDetector.DetectByMAD(counts),
            "z-score" => AnomalyDetector.DetectByZScore(counts),
            _ => throw new ArgumentException("Invalid mode. Use 'z-score' or 'mad'")
        };

        var anomalies = anomalyIndexes.Select(i => new AnomalyPointDto
        {
            Timestamp = raw[i].Timestamp,
            Count = raw[i].Count,
            Reason = $"Detected by {mode}"
        }).ToList();

        _logger.LogInformation("Anomaly detection found {Count} anomalies", anomalies.Count);

        return new AnomalyReportDto
        {
            DetectionMode = mode.ToLower(),
            RangeStart = from,
            RangeEnd = to,
            Anomalies = anomalies
        };
    }

    /// <summary>
    /// Computes daily usage trends across a specified time window.
    /// Includes totals, averages, moving average and percentage changes.
    /// </summary>
    /// <param name="from">Start of trend period.</param>
    /// <param name="to">End of trend period.</param>
    /// <returns>A structured <see cref="TrendReportDto"/> containing trend data.</returns>
    public async Task<TrendReportDto> GetDailyTrendsAsync(DateTime from, DateTime to)
    {
        var data = (await _repo.GetDailyCountsAsync(from, to)).ToList();

        if (!data.Any())
        {
            _logger.LogWarning("No daily trend data found between {From} and {To}", from, to);

            return new TrendReportDto
            {
                RangeLabel = $"{from:yyyy-MM-dd} - {to:yyyy-MM-dd}",
                Points = [],
                TotalCount = 0,
                AveragePerPoint = 0
            };
        }

        var counts = data.Select(x => x.Count).ToList();
        var total = counts.Sum();
        var average = total / (double)data.Count;

        var movingAvg = TrendMath.MovingAverage(counts, 3);
        var percentage = TrendMath.PercentageChanges(counts);

        _logger.LogInformation(
            "Daily trend computed: {Points} points, total: {Total}, avg: {Avg:F2}, movingAvg(3): {MA}, %change: {PC}",
            data.Count, total, average, movingAvg, percentage
        );

        return new TrendReportDto
        {
            RangeLabel = $"{from:yyyy-MM-dd} - {to:yyyy-MM-dd}",
            Points = data,
            TotalCount = total,
            AveragePerPoint = average,
            MovingAverage = movingAvg,
            PercentageChange = percentage
        };
    }

    /// <summary>
    /// Computes monthly aggregated usage trends for a given year.
    /// Includes totals, averages, moving averages and percentage changes.
    /// </summary>
    /// <param name="year">The target year.</param>
    /// <returns>A <see cref="TrendReportDto"/> summarizing monthly trends.</returns>
    public async Task<TrendReportDto> GetMonthlyTrendsAsync(int year)
    {
        var data = (await _repo.GetMonthlyCountsAsync(year)).ToList();

        if (!data.Any())
        {
            _logger.LogWarning("No monthly trend data found for {Year}", year);

            return new TrendReportDto
            {
                RangeLabel = $"{year}",
                Points = [],
                TotalCount = 0,
                AveragePerPoint = 0
            };
        }

        var count = data.Select(x => x.Count).ToList();
        var total = count.Sum();
        var average = total / (double)data.Count;

        var movingAvg = TrendMath.MovingAverage(count, 3);
        var percentage = TrendMath.PercentageChanges(count);

        _logger.LogInformation(
            "Monthly trend computed: {Points} months, total {Total}, avg {Avg:F2}, movingAvg(3): {MA}, %change: {PC}",
            data.Count, total, average, movingAvg, percentage
        );

        return new TrendReportDto
        {
            RangeLabel = $"{year}",
            Points = data,
            TotalCount = total,
            AveragePerPoint = average,
            MovingAverage = movingAvg,
            PercentageChange = percentage
        };
    }

    /// <summary>
    /// Determines the peak usage count within a date range.
    /// </summary>
    /// <param name="from">Start of the analysis period.</param>
    /// <param name="to">End of the analysis period.</param>
    /// <returns>Metadata describing when peak usage occurred.</returns>
    public async Task<PeakUsageDto> GetPeakUsageAsync(DateTime from, DateTime to)
    {
        var data = (await _repo.GetDailyCountsAsync(from, to)).ToList();

        if (!data.Any())
        {
            _logger.LogWarning("No data found for peak usage between {From} and {To}", from, to);

            return new PeakUsageDto
            {
                PeakTimestamp = DateTime.MinValue,
                PeakCount = 0,
                Grouping = "Day"
            };
        }

        var peak = data.MaxBy(x => x.Count)!;

        _logger.LogInformation("Peak usage: {Count} events on {Date}", peak.Count, peak.Timestamp.ToShortDateString());

        return new PeakUsageDto
        {
            PeakTimestamp = peak.Timestamp,
            PeakCount = peak.Count,
            Grouping = "Day"
        };
    }

    /// <summary>
    /// Builds visualization-friendly data for usage charts such as line graphs.
    /// </summary>
    /// <param name="from">Start of range.</param>
    /// <param name="to">End of range.</param>
    /// <returns>A <see cref="VisualizationDataDto"/> containing labels and values.</returns>
    public async Task<VisualizationDataDto> GetVisualizationDataAsync(DateTime from, DateTime to)
    {
        var data = (await _repo.GetDailyCountsAsync(from, to)).ToList();

        if (!data.Any())
        {
            _logger.LogWarning("No visualization data found for requested period");

            return new VisualizationDataDto
            {
                Labels = [],
                Values = [],
                ChartType = "line"
            };
        }

        var labels = data.Select(x => x.Timestamp.ToString("MM-dd")).ToList();
        var values = data.Select(x => x.Count).ToList();

        _logger.LogInformation("Visualization data created: {N} points", labels.Count);

        return new VisualizationDataDto
        {
            Labels = labels,
            Values = values,
            ChartType = "line"
        };
    }
}