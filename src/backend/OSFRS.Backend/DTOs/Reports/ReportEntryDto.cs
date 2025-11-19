namespace OSFRS.Backend.DTOs;

public record ReportEntryDto
{
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } 
    public string? Metadata { get; set; }
}