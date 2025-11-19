namespace OSFRS.Backend.DTOs.Reports;

public record ReportEntryDto
{
    public string EventType { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public string? Metadata { get; init; }
}