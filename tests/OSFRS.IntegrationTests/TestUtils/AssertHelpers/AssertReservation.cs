using FluentAssertions;
using OSFRS.Models.Entities;

namespace OSFRS.IntegrationTests.TestUtils.AssertHelpers;

public static class AssertReservation
{
    public static void Equal(
        Reservation? actual,
        Reservation expected,
        bool includeUser = false,
        bool includeFacility = false
    )
    {
        actual.Should().NotBeNull();

        actual!.Id.Should().Be(expected.Id);
        actual.UserId.Should().Be(expected.UserId);
        actual.FacilityId.Should().Be(expected.FacilityId);

        actual.StartTime.Should().Be(expected.StartTime);
        actual.EndTime.Should().Be(expected.EndTime);

        actual.Status.Should().Be(expected.Status);

        actual.CreatedAt.Should().BeCloseTo(expected.CreatedAt, TimeSpan.FromSeconds(1));
        actual.UpdatedAt.Should().BeCloseTo(expected.UpdatedAt, TimeSpan.FromSeconds(1));

        if (includeUser && expected.User is not null)
            AssertUser.Equal(actual.User, expected.User);

        if (includeFacility && expected.Facility is not null)
            AssertFacility.Equal(actual.Facility, expected.Facility);
    }
}
