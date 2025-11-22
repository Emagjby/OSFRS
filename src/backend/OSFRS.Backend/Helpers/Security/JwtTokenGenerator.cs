using System.IdentityModel.Tokens.Jwt;
using JwtClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using OSFRS.Models.Entities;
using OSFRS.Backend.Interfaces.Helper;

namespace OSFRS.Backend.Helpers;

/// <summary>
/// Generates JSON Web Tokens (JWT) for authenticated users.
/// </summary>
/// <remarks>
/// This implementation loads all configuration values from environment variables:
/// <list type="bullet">
/// <item><description><c>JWT_SECRET</c> – HMAC signing secret (required)</description></item>
/// <item><description><c>JWT_ISSUER</c> – token issuer</description></item>
/// <item><description><c>JWT_AUDIENCE</c> – token audience</description></item>
/// <item><description><c>JWT_EXPIRY_MINUTES</c> – optional override for default expiry</description></item>
/// </list>
/// </remarks>
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _defaultExpiryMinutes;

    /// <summary>
    /// Initializes a new <see cref="JwtTokenGenerator"/> by loading all
    /// required configuration values from environment variables.
    /// </summary>
    /// <exception cref="Exception">
    /// Thrown if any required environment variable is not set.
    /// </exception>
    public JwtTokenGenerator()
    {
        _secret = Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? throw new Exception("JWT_SECRET not set.");

        _issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
            ?? throw new Exception("JWT_ISSUER not set.");

        _audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
            ?? throw new Exception("JWT_AUDIENCE not set.");

        _defaultExpiryMinutes =
            int.TryParse(Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES"), out int exp)
                ? exp
                : 60;
    }

    /// <summary>
    /// Generates a signed JWT for the specified user.
    /// </summary>
    /// <param name="user">The authenticated user.</param>
    /// <param name="expiryInMinutes">Optional custom expiry duration.</param>
    /// <returns>A signed JWT string.</returns>
    public string GenerateToken(User user, int? expiryInMinutes = null)
    {
        Claim[] claims =
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtClaimNames.UniqueName, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtClaimNames.Email, user.Email),
            new Claim(JwtClaimNames.Jti, Guid.NewGuid().ToString())
        ];

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiryMinutes = expiryInMinutes ?? _defaultExpiryMinutes;

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}