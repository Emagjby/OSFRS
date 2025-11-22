namespace OSFRS.Backend.DTOs.Reports;

/// <summary>
/// Represents a single entry in a generated report.
/// Includes the event type, timestamp, and optional metadata payload.
/// </summary>
public record ReportEntryDto
{
    /// <summary>
    /// The type of event recorded (for example: ReservationCreated, FacilityUpdated, etc.).
    /// </summary>
    public string EventType { get; init; } = string.Empty;

    /// <summary>
    /// The timestamp when the event occurred (UTC).
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Optional serialized metadata providing additional context for the event.
    /// May be null if no metadata was attached.
    /// </summary>
    public string? Metadata { get; init; }
}