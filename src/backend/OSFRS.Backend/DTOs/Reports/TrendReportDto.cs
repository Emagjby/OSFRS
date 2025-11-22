using OSFRS.Backend.DTOs.Analytics;

namespace OSFRS.Backend.DTOs.Reports;

/// <summary>
/// Represents a trend analysis report over a specified range,
/// including raw trend points and computed statistical metrics.
/// </summary>
public record TrendReportDto
{
    /// <summary>
    /// A human-readable label describing the analyzed range
    /// (for example: "Last 7 Days", "January 2025", "Q1 2025").
    /// </summary>
    public required string RangeLabel { get; init; }

    /// <summary>
    /// A chronological collection of measured data points used
    /// to construct the trend.
    /// </summary>
    public IEnumerable<TrendPointDto> Points { get; init; } = [];

    /// <summary>
    /// The total count of events across the entire trend range.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// The computed average number of events per trend point.
    /// </summary>
    public double AveragePerPoint { get; init; }

    /// <summary>
    /// Optional moving-average smoothing values aligned with the Points collection.
    /// </summary>
    public IEnumerable<double>? MovingAverage { get; init; }

    /// <summary>
    /// Optional percentage change values between each consecutive trend point.
    /// </summary>
    public IEnumerable<double>? PercentageChange { get; init; }
}