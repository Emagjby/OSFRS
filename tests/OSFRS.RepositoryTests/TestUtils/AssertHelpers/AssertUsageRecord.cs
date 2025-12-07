using FluentAssertions;
using OSFRS.Models.Entities;

namespace OSFRS.RepositoryTests.TestUtils.AssertHelpers;

public static class AssertUsageRecord
{
    public static void Equal(UsageRecord actual, UsageRecord expected)
    {
        actual.Should().NotBeNull();
        actual.Id.Should().Be(expected.Id);
        actual.EventType.Should().Be(expected.EventType);
        actual.Timestamp.Should().Be(expected.Timestamp);
        actual.AggregatedData.Should().Be(expected.AggregatedData);
        actual.FacilityId.Should().Be(expected.FacilityId);
        actual.UserId.Should().Be(expected.UserId);
    }
}
