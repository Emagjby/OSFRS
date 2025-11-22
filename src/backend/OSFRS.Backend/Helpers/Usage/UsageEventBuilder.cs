using OSFRS.Backend.DTOs.Analytics;

namespace OSFRS.Backend.Helpers.Usage;

/// <summary>
/// Provides helper methods for constructing <see cref="UsageEventDto"/> objects.
/// </summary>
/// <remarks>
/// This builder ensures all usage events are created with a consistent structure
/// and automatically assigns the current UTC timestamp.
/// </remarks>
public static class UsageEventBuilder
{
    /// <summary>
    /// Creates a new <see cref="UsageEventDto"/> with optional user, facility, and metadata fields.
    /// </summary>
    /// <param name="eventType">The type of event occurring (must match <see cref="UsageEventTypes"/>).</param>
    /// <param name="userId">Optional user identifier associated with the event.</param>
    /// <param name="facilityId">Optional facility identifier associated with the event.</param>
    /// <param name="metadata">Optional key-value metadata describing event context.</param>
    /// <returns>A fully populated <see cref="UsageEventDto"/> with a UTC timestamp.</returns>
    public static UsageEventDto Create(
        string eventType,
        int? userId = null,
        int? facilityId = null,
        Dictionary<string, string>? metadata = null)
    {
        return new UsageEventDto
        {
            EventType = eventType,
            UserId = userId,
            FacilityId = facilityId,
            Timestamp = DateTime.UtcNow,
            Metadata = metadata
        };
    }

    /// <summary>
    /// Creates a <see cref="UsageEventDto"/> using an object-based metadata dictionary,
    /// automatically converting values to strings.
    /// </summary>
    /// <param name="eventType">The type of event occurring.</param>
    /// <param name="metadataObj">Metadata values where values may be boxed as any object type.</param>
    /// <param name="userId">Optional user identifier.</param>
    /// <param name="facilityId">Optional facility identifier.</param>
    /// <returns>A <see cref="UsageEventDto"/> with serialized metadata values.</returns>
    public static UsageEventDto CreateWithMetadata(
        string eventType,
        Dictionary<string, object> metadataObj,
        int? userId = null,
        int? facilityId = null)
    {
        return new UsageEventDto
        {
            EventType = eventType,
            UserId = userId,
            FacilityId = facilityId,
            Timestamp = DateTime.UtcNow,
            Metadata = metadataObj.ToDictionary(
                x => x.Key,
                x => x.Value?.ToString() ?? string.Empty)
        };
    }
}