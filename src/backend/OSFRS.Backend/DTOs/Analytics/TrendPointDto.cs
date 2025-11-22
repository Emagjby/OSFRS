namespace OSFRS.Backend.DTOs.Analytics;

/// <summary>
/// Represents a single data point in a usage trend analysis.
/// Each point links a moment in time with the activity count observed at that moment.
/// </summary>
public record TrendPointDto
{
    /// <summary>
    /// The timestamp associated with the trend measurement.
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// The usage count recorded at the given timestamp.
    /// </summary>
    public int Count { get; init; }
}