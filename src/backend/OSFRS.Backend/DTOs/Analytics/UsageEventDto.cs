namespace OSFRS.Backend.DTOs.Analytics;

/// <summary>
/// Represents an individual usage event emitted by the system.
/// Usage events are the fundamental telemetry units used for analytics,
/// reporting, anomaly detection, and system behavior auditing.
/// </summary>
public record UsageEventDto
{
    /// <summary>
    /// The event type identifier describing what occurred.
    /// Must match one of the values defined in <c>UsageEventTypes</c>.
    /// </summary>
    public string EventType { get; init; } = null!;

    /// <summary>
    /// The ID of the user associated with the event, if applicable.
    /// Some events are system-generated and may not include a user.
    /// </summary>
    public int? UserId { get; init; }

    /// <summary>
    /// The ID of the facility related to the event, if applicable.
    /// </summary>
    public int? FacilityId { get; init; }

    /// <summary>
    /// The timestamp indicating when the event occurred.
    /// Automatically set to <see cref="DateTime.UtcNow"/> unless overridden.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Optional metadata providing additional event-specific details.
    /// Useful for storing contextual values such as ReservationId or availability changes.
    /// </summary>
    public Dictionary<string, string>? Metadata { get; init; }
}