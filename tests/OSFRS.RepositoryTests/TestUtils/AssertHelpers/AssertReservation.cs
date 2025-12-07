using FluentAssertions;
using OSFRS.Models.Entities;

namespace OSFRS.RepositoryTests.TestUtils.AssertHelpers;

public static class AssertReservation
{
    public static void Equal(Reservation actual, Reservation expected)
    {
        actual.Should().NotBeNull();
        actual.Id.Should().Be(expected.Id);
        actual.UserId.Should().Be(expected.UserId);
        actual.FacilityId.Should().Be(expected.FacilityId);
        actual.StartTime.Should().Be(expected.StartTime);
        actual.EndTime.Should().Be(expected.EndTime);
        actual.Status.Should().Be(expected.Status);
    }
}
