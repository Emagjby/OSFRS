using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using OSFRS.Models.Entities;
using OSFRS.SecurityTests.Utils;

public class InvalidTokenTests : SecurityTestBase
{
    public InvalidTokenTests(SecurityWebAppFactory factory)
        : base(factory) { }

    private const string EP = "/api/facility";

    // --------------------------------------------------------
    // 1. No token → 401
    // --------------------------------------------------------
    [Fact]
    public async Task No_Token_Returns_401()
    {
        var res = await Anonymous.GetAsync(EP);
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --------------------------------------------------------
    // 2. Garbage token → 401
    // --------------------------------------------------------
    [Fact]
    public async Task Garbage_Token_Returns_401()
    {
        var client = Clients.CreateClientWithToken("garbage.token.value");
        var res = await client.GetAsync(EP);

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --------------------------------------------------------
    // 3. Wrong issuer → 401
    // --------------------------------------------------------
    [Fact]
    public async Task Wrong_Issuer_Returns_401()
    {
        var token = Clients.TokenGenerator.GenerateToken(
            new User
            {
                Id = 777,
                Username = "wrongissuer",
                Email = "x@test.com",
                Role = "User",
            }
        );

        AppFactory.JwtOverride = p =>
        {
            p.ValidateIssuer = true;
            p.ValidIssuer = "TotallyWrongIssuer";
            p.ValidIssuers = new[] { "TotallyWrongIssuer" }; // important
        };

        var client = Clients.CreateClientWithToken(token);
        var res = await client.GetAsync(EP);

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --------------------------------------------------------
    // 4. Wrong audience → 401
    // --------------------------------------------------------
    [Fact]
    public async Task Wrong_Audience_Returns_401()
    {
        AppFactory.JwtOverride = p =>
        {
            p.ValidateAudience = true;
            p.ValidAudience = "WrongAudience";
            p.ValidAudiences = new[] { "WrongAudience" }; // important
        };

        var token = Clients.TokenGenerator.GenerateToken(
            new User
            {
                Id = 99,
                Username = "auduser",
                Email = "aud@test.com",
                Role = "User",
            }
        );

        var client = Clients.CreateClientWithToken(token);
        var res = await client.GetAsync(EP);

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --------------------------------------------------------
    // 5. Expired token → 401
    // --------------------------------------------------------
    [Fact]
    public async Task Expired_Token_Returns_401()
    {
        var expiredToken = Clients.TokenGenerator.GenerateToken(
            new User
            {
                Id = 42,
                Username = "expired",
                Email = "expired@test.com",
                Role = "User",
            },
            expiryInMinutes: -120
        );

        var client = Clients.CreateClientWithToken(expiredToken);
        var res = await client.GetAsync(EP);

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --------------------------------------------------------
    // 6. Wrong signature → 401
    // --------------------------------------------------------
    [Fact]
    public async Task Wrong_Signature_Returns_401()
    {
        var normalToken = Clients.TokenGenerator.GenerateToken(
            new User
            {
                Id = 200,
                Username = "signedWrong",
                Email = "wrong@key.com",
                Role = "User",
            }
        );

        AppFactory.JwtOverride = p =>
        {
            p.IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("INVALID_SECRET_123!")
            );

            p.IssuerSigningKeyResolver = null;
        };

        var client = Clients.CreateClientWithToken(normalToken);
        var res = await client.GetAsync(EP);

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
