namespace OSFRS.Backend.DTOs.Analytics;

/// <summary>
/// Represents an aggregated analytics result computed over a defined time window.
/// Contains grouped event counts filtered by event type, user, or facility.
/// </summary>
public record UsageAggregateDto
{
    /// <summary>
    /// The type of event being aggregated 
    /// (for example: ReservationCreated, FacilityUpdated, etc.).
    /// </summary>
    public string EventType { get; init; } = null!;

    /// <summary>
    /// The number of occurrences of the event within the aggregation window.
    /// </summary>
    public int Count { get; init; }

    /// <summary>
    /// The start timestamp of the aggregation period (inclusive).
    /// </summary>
    public DateTime PeriodStart { get; init; }

    /// <summary>
    /// The end timestamp of the aggregation period (exclusive).
    /// </summary>
    public DateTime PeriodEnd { get; init; }

    /// <summary>
    /// Optional filter indicating the user associated with the aggregated data.
    /// </summary>
    public int? UserId { get; init; }

    /// <summary>
    /// Optional filter indicating the facility associated with the aggregated data.
    /// </summary>
    public int? FacilityId { get; init; }
}