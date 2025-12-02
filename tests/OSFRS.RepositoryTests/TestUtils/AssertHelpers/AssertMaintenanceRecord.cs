using FluentAssertions;
using OSFRS.Models.Entities;

namespace OSFRS.RepositoryTests.TestUtils.AssertHelpers;

public static class AssertMaintenanceRecord
{
    public static void Equal(MaintenanceRecord actual, MaintenanceRecord expected)
    {
        actual.Should().NotBeNull();
        actual.Id.Should().Be(expected.Id);
        actual.FacilityId.Should().Be(expected.FacilityId);
        actual.StartTime.Should().Be(expected.StartTime);
        actual.EndTime.Should().Be(expected.EndTime);
        actual.Status.Should().Be(expected.Status);
    }
}
