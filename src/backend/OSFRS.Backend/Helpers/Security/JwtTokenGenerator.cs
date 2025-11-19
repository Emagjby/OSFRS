using System;
using System.IdentityModel.Tokens.Jwt;
using JwtClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using OSFRS.Models.Entities;
using OSFRS.Backend.Interfaces;


namespace OSFRS.Backend.Helpers;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _defaultExpiryMinutes;

    public JwtTokenGenerator()
    {
        //Read env vars
        _secret = Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? throw new Exception("JWT_SECRET not set.");
        _issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
            ?? throw new Exception("JWT_ISSUER not set.");
        _audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
            ?? throw new Exception("JWT_AUDIENCE not set.");

        _defaultExpiryMinutes = int.TryParse(Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES"), out int exp) ? exp : 60;
    }

    public string GenerateToken(User user, int? expiryInMinutes = null)
    {
        Claim[] claims = [
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