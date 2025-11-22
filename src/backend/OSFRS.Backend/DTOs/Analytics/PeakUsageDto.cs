namespace OSFRS.Backend.DTOs.Analytics;

/// <summary>
/// Represents the highest recorded usage value within a given analysis interval.
/// Used to highlight peak demand periods for facilities, users, or time-based segments.
/// </summary>
public record PeakUsageDto
{
    /// <summary>
    /// The timestamp at which the peak usage was observed.
    /// </summary>
    public DateTime PeakTimestamp { get; init; }

    /// <summary>
    /// The measured usage count at the peak moment.
    /// </summary>
    public int PeakCount { get; init; }

    /// <summary>
    /// Describes the grouping dimension used for the analysis, 
    /// such as "Hour", "Day", "Facility", or another aggregate key.
    /// </summary>
    public required string Grouping { get; init; }
}