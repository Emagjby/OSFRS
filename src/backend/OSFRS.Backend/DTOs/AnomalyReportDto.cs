namespace OSFRS.Backend.DTOs;

public record AnomalyReportDto
{
    public IEnumerable<AnomalyPointDto> Anomalies { get; set; } = [];
    public required string DetectionMode { get; set; } //med, z-score
    public DateTime RangeStart { get; set; }
    public DateTime RangeEnd { get; set; }
}