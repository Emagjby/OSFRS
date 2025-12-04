using FluentAssertions;
using OSFRS.Models.Entities;

namespace OSFRS.IntegrationTests.TestUtils.AssertHelpers;

public static class AssertMaintenance
{
    public static void Equal(MaintenanceRecord? actual, MaintenanceRecord expected)
    {
        actual.Should().NotBeNull();

        actual!.Id.Should().Be(expected.Id);
        actual.FacilityId.Should().Be(expected.FacilityId);
        actual.Description.Should().Be(expected.Description);
        actual.StartTime.Should().Be(expected.StartTime);
        actual.EndTime.Should().Be(expected.EndTime);
        actual.Status.Should().Be(expected.Status);

        actual.CreatedAt.Should().BeCloseTo(expected.CreatedAt, TimeSpan.FromSeconds(1));
        actual.UpdatedAt.Should().BeCloseTo(expected.UpdatedAt, TimeSpan.FromSeconds(1));
    }
}
