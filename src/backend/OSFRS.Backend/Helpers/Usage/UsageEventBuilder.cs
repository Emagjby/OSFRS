using OSFRS.Backend.DTOs.Analytics;

namespace OSFRS.Backend.Helpers.Usage;

public static class UsageEventBuilder
{
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