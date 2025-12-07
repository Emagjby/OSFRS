using System.Security.Claims;
using FluentAssertions;
using OSFRS.Backend.Helpers;
using static OSFRS.UnitTests.TestUtils.HelperTestHelpers;

namespace OSFRS.UnitTests.Helpers;

public class JwtTokenGeneratorTests
{
    public JwtTokenGeneratorTests()
    {
        // Set controlled environment variables for the generator
        Environment.SetEnvironmentVariable(
            "JWT_SECRET",
            "gxrJ/2Od3ZDlqBptnDUs9rWgECnIdIvSOVqjXGeaXA4="
        );
        Environment.SetEnvironmentVariable("JWT_ISSUER", "TestIssuer");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "TestAudience");
        Environment.SetEnvironmentVariable("JWT_EXPIRY_MINUTES", "30");
    }

    // ------------------------------------------------------------
    // CLAIMS EXIST
    // ------------------------------------------------------------
    [Fact]
    public void GenerateToken_ShouldContainExpectedClaims()
    {
        var gen = new JwtTokenGenerator();
        var tokenStr = gen.GenerateToken(FakeUser);
        var token = Decode(tokenStr);

        token.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value.Should().Be("42");

        token.Claims.First(c => c.Type == "sub").Value.Should().Be("42");

        token.Claims.First(c => c.Type == "unique_name").Value.Should().Be("gencho");

        token.Claims.First(c => c.Type == ClaimTypes.Role).Value.Should().Be("Admin");

        token.Claims.First(c => c.Type == "email").Value.Should().Be("gencho@test.com");
    }

    // ------------------------------------------------------------
    // EXPIRATION CORRECTNESS
    // ------------------------------------------------------------
    [Fact]
    public void GenerateToken_ShouldSetCorrectExpiration()
    {
        var gen = new JwtTokenGenerator();

        var before = TrimToSeconds(DateTime.UtcNow);
        var tokenStr = gen.GenerateToken(FakeUser, expiryInMinutes: 15);
        var after = TrimToSeconds(DateTime.UtcNow);

        var token = Decode(tokenStr);

        token.ValidTo.Should().BeOnOrAfter(before.AddMinutes(15));
        token.ValidTo.Should().BeOnOrBefore(after.AddMinutes(15));
    }

    // ------------------------------------------------------------
    // SIGNATURE MUST CHANGE WHEN SECRET CHANGES
    // ------------------------------------------------------------
    [Fact]
    public void GenerateToken_ShouldChangeSignature_WhenSecretChanges()
    {
        // FIRST TOKEN
        Environment.SetEnvironmentVariable(
            "JWT_SECRET",
            "JiZbDkOGyMjCnxXi5yGsmYTYTyyz45aQkkQQmeCfQpk="
        );
        var genA = new JwtTokenGenerator();
        var tokenA = genA.GenerateToken(FakeUser);

        // SECOND TOKEN WITH DIFFERENT SECRET
        Environment.SetEnvironmentVariable(
            "JWT_SECRET",
            "xWy4y0ZeVCF3PhCOVR/tE4YvfLnhJpO/Wa1uv4vVtrg="
        );
        var genB = new JwtTokenGenerator();
        var tokenB = genB.GenerateToken(FakeUser);

        tokenA.Should().NotBe(tokenB);
    }

    // ------------------------------------------------------------
    // ROLE CLAIM INCLUDED
    // ------------------------------------------------------------
    [Fact]
    public void GenerateToken_ShouldIncludeRoleClaim()
    {
        var gen = new JwtTokenGenerator();

        var token = Decode(gen.GenerateToken(FakeUser));

        var roleClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
        roleClaim.Should().NotBeNull();
        roleClaim!.Value.Should().Be("Admin");
    }
}
