using OSFRS.Models.Entities;

namespace OSFRS.RepositoryTests.TestUtils.EntityBuilders;

public static class UsageRecordBuilder
{
    private static int _nextId = 1;

    public static UsageRecord Create(
        int? id = null,
        string eventType = "ReservationCreated",
        DateTime? timestamp = null,
        string aggregatedData = "{}",
        int? userId = null,
        int? facilityId = null
    )
    {
        int finalId = id ?? _nextId++;

        return new UsageRecord
        {
            Id = finalId,
            EventType = eventType,
            Timestamp = timestamp ?? DateTime.UtcNow,
            AggregatedData = aggregatedData,
            UserId = userId,
            FacilityId = facilityId,
        };
    }
}
