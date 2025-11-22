namespace OSFRS.Backend.DTOs.Analytics;

/// <summary>
/// Represents a single anomalous data point detected during analytics processing.
/// Used for highlighting irregular usage spikes, drops, or outliers.
/// </summary>
public record AnomalyPointDto
{
    /// <summary>
    /// The exact moment in time when the anomaly was observed.
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// The measured count or value that triggered anomaly detection.
    /// </summary>
    public int Count { get; init; }

    /// <summary>
    /// A short explanation describing why this data point was flagged as anomalous.
    /// </summary>
    public string Reason { get; init; } = string.Empty;
}