using FluentAssertions;
using OSFRS.Models.Entities;

namespace OSFRS.RepositoryTests.TestUtils.AssertHelpers;

public static class AssertFacility
{
    public static void Equal(Facility actual, Facility expected)
    {
        actual.Should().NotBeNull();
        actual.Id.Should().Be(expected.Id);
        actual.Name.Should().Be(expected.Name);
        actual.Type.Should().Be(expected.Type);
        actual.Capacity.Should().Be(expected.Capacity);
        actual.Status.Should().Be(expected.Status);
    }
}
