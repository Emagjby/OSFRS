using FluentAssertions;
using OSFRS.Models.Entities;

namespace OSFRS.IntegrationTests.TestUtils.AssertHelpers;

public static class AssertUser
{
    public static void Equal(User? actual, User expected)
    {
        actual.Should().NotBeNull();

        actual!.Id.Should().Be(expected.Id);
        actual.Name.Should().Be(expected.Name);
        actual.Username.Should().Be(expected.Username);
        actual.Email.Should().Be(expected.Email);
        actual.PasswordHash.Should().Be(expected.PasswordHash);
        actual.Role.Should().Be(expected.Role);

        actual.CreatedAt.Should().BeCloseTo(expected.CreatedAt, TimeSpan.FromSeconds(1));
        actual.UpdatedAt.Should().BeCloseTo(expected.UpdatedAt, TimeSpan.FromSeconds(1));
    }
}
