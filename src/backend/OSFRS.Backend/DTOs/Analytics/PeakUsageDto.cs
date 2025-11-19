namespace OSFRS.Backend.DTOs.Analytics;

public record PeakUsageDto
{
    public DateTime PeakTimestamp { get; init; }
    public int PeakCount { get; init; }

    public required string Grouping { get; init; } // examples: Hour, Day, Facility...
}