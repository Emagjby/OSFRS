using OSFRS.Backend.DTOs.Analytics;

namespace OSFRS.Backend.DTOs.Reports;

/// <summary>
/// Represents the result of anomaly detection over a specified time range.
/// Contains all detected anomaly points, the mode used for detection,
/// and the boundaries of the analyzed period.
/// </summary>
public record AnomalyReportDto
{
    /// <summary>
    /// Collection of detected anomalies containing timestamps, counts, and reasons.
    /// </summary>
    public IEnumerable<AnomalyPointDto> Anomalies { get; init; } = [];

    /// <summary>
    /// The detection method used, for example: "z-score" or "mad".
    /// </summary>
    public required string DetectionMode { get; init; }

    /// <summary>
    /// The start timestamp of the analyzed range (UTC).
    /// </summary>
    public DateTime RangeStart { get; init; }

    /// <summary>
    /// The end timestamp of the analyzed range (UTC).
    /// </summary>
    public DateTime RangeEnd { get; init; }
}