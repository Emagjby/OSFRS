using OSFRS.Backend.DTOs.Analytics;

namespace OSFRS.Backend.DTOs.Reports;

public record TrendReportDto
{
    public required string RangeLabel { get; init; }
    public IEnumerable<TrendPointDto> Points { get; init; } = [];
    public int TotalCount { get; init; }
    public double AveragePerPoint { get; init; }
    public IEnumerable<double>? MovingAverage { get; init; }
    public IEnumerable<double>? PercentageChange { get; init; }
}