namespace OSFRS.Backend.DTOs;

public record PeakUsageDto
{
    public DateTime PeakTimestamp { get; set; }
    public int PeakCount { get; set; }

    public required string Grouping { get; set; } // examples: Hour, Day, Facility...
}