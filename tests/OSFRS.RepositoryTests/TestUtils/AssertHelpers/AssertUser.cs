using FluentAssertions;
using OSFRS.Models.Entities;

namespace OSFRS.RepositoryTests.TestUtils.AssertHelpers;

public static class AssertUser
{
    public static void Equal(User actual, User expected)
    {
        actual.Should().NotBeNull();
        actual.Id.Should().Be(expected.Id);
        actual.Username.Should().Be(expected.Username);
        actual.Email.Should().Be(expected.Email);
        actual.Role.Should().Be(expected.Role);
        actual.Name.Should().Be(expected.Name);
    }
}
