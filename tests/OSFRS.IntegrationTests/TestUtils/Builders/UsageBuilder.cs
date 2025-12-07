using OSFRS.Models.Entities;

namespace OSFRS.IntegrationTests.TestUtils.Builders;

public class UsageBuilder
{
    private string _type = "Event";
    private int? _userId = null;
    private int? _facilityId = null;
    private DateTime _timestamp = DateTime.UtcNow;
    private string? _data = null;

    public static UsageBuilder Create() => new();

    public UsageBuilder ForUser(int? userId)
    {
        _userId = userId;
        return this;
    }

    public UsageBuilder ForFacility(int? facilityId)
    {
        _facilityId = facilityId;
        return this;
    }

    public UsageBuilder WithType(string type)
    {
        _type = type;
        return this;
    }

    public UsageBuilder At(DateTime timestamp)
    {
        _timestamp = timestamp;
        return this;
    }

    public UsageBuilder WithAggregatedData(string? data)
    {
        _data = data;
        return this;
    }

    public UsageRecord Build() =>
        new UsageRecord
        {
            EventType = _type,
            UserId = _userId,
            FacilityId = _facilityId,
            Timestamp = _timestamp,
            AggregatedData = _data,
        };
}
