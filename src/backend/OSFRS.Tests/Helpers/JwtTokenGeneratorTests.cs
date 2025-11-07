using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using OSFRS.Backend.Helpers;
using OSFRS.Models.Entities;
using OSFRS.Backend.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace OSFRS.Tests.Helpers;

public class JwtTokenGeneratorTests : IDisposable
{
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    
    private readonly string? _oldSecret;
    private readonly string? _oldIssuer;
    private readonly string? _oldAudience;


    public JwtTokenGeneratorTests()
    {
        _oldSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
        _oldIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
        _oldAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

        Environment.SetEnvironmentVariable("JWT_SECRET", "Xo8pCrcllE87HPhyaBbR6bo2gN0gh/obKNGBhVb1r1U=");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "TestIssuer");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "TestAudience");

        _jwtTokenGenerator = new JwtTokenGenerator();
    }

    public void Dispose()
    {
        if (_oldSecret is not null)
            Environment.SetEnvironmentVariable("JWT_SECRET", _oldSecret);

        if (_oldIssuer is not null)
            Environment.SetEnvironmentVariable("JWT_ISSUER", _oldIssuer);
            
        if(_oldAudience is not null)
            Environment.SetEnvironmentVariable("JWT_AUDIENCE", _oldAudience);
    }

    [Fact]
    public void GenerateToken_ShouldReturnNonNullToken()
    {
        var user = new User
        {
            Id = 1,
            Username = "johndoe",
            Email = "john@example.com",
            Role = "User"
        };

        string token = _jwtTokenGenerator.GenerateToken(user);

        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public void GenerateToken_ShouldContainCorrectClaims()
    {
        var user = new User
        {
            Id = 1,
            Username = "johndoe",
            Email = "john@example.com",
            Role = "User"
        };

        string token = _jwtTokenGenerator.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Equal(user.Id.ToString(), jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal(user.Username, jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.UniqueName).Value);
        Assert.Equal(user.Role, jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);
        Assert.Equal(user.Email, jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
    }

    [Fact]
    public void GenerateToken_ShouldBeValidatableWithCorrectSecret()
    {
        var user = new User
        {
            Id = 1,
            Username = "johndoe",
            Email = "john@example.com",
            Role = "User"
        };

        string token = _jwtTokenGenerator.GenerateToken(user);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Xo8pCrcllE87HPhyaBbR6bo2gN0gh/obKNGBhVb1r1U="));
        var tokenHandler = new JwtSecurityTokenHandler();
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "TestIssuer",
            ValidAudience = "TestAudience",
            IssuerSigningKey = key
        };

        var principal = tokenHandler.ValidateToken(token, parameters, out var validatedToken);

        Assert.NotNull(principal);
        Assert.Equal(user.Username, principal.Identity!.Name);
    }

    [Fact]
    public void GenerateToken_WithExpiredTime_ShouldFailValidation()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Name = "Expired User",
            Username = "expired_user",
            Email = "expired@example.com",
            Role = "User"
        };

        var expiredToken = _jwtTokenGenerator.GenerateToken(user, -1);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("Xo8pCrcllE87HPhyaBbR6bo2gN0gh/obKNGBhVb1r1U=");

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // Act + Assert
        Assert.Throws<SecurityTokenExpiredException>(() =>
        {
            tokenHandler.ValidateToken(expiredToken, validationParams, out var _);
        });
    }
}