using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using OSFRS.Backend.Helpers.Auth;

namespace OSFRS.UnitTests.Helpers.Auth;

public class UserContextHelperTests
{
    // ---------------------------------------------------------
    // 1. Extract from "sub" (JWT standard claim)
    // ---------------------------------------------------------
    [Fact]
    public void GetUserId_ShouldReadFromSubClaim()
    {
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, "42") };

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var result = UserContextHelper.GetUserId(principal);

        result.Should().Be(42);
    }

    // ---------------------------------------------------------
    // 2. Extract from NameIdentifier if "sub" is missing
    // ---------------------------------------------------------
    [Fact]
    public void GetUserId_ShouldReadFromNameIdentifier_WhenSubMissing()
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "99") };

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var result = UserContextHelper.GetUserId(principal);

        result.Should().Be(99);
    }

    // ---------------------------------------------------------
    // 3. Missing claims returns null
    // ---------------------------------------------------------
    [Fact]
    public void GetUserId_ShouldReturnNull_WhenNoRelevantClaims()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        var result = UserContextHelper.GetUserId(principal);

        result.Should().BeNull();
    }

    // ---------------------------------------------------------
    // 4. Non-integer claim returns null
    // ---------------------------------------------------------
    [Fact]
    public void GetUserId_ShouldReturnNull_WhenClaimNotNumeric()
    {
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, "abc") };

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var result = UserContextHelper.GetUserId(principal);

        result.Should().BeNull();
    }
}
