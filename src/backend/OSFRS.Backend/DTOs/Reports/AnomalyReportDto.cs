using OSFRS.Backend.DTOs.Analytics;

namespace OSFRS.Backend.DTOs.Reports;

public record AnomalyReportDto
{
    public IEnumerable<AnomalyPointDto> Anomalies { get; init; } = [];
    public required string DetectionMode { get; init; }
    public DateTime RangeStart { get; init; }
    public DateTime RangeEnd { get; init; }
}