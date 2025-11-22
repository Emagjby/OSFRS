using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces.Helper;

/// <summary>
/// Provides JWT token generation capabilities for authenticated users.
/// </summary>
/// <remarks>
/// Implementations of this interface create signed JSON Web Tokens (JWTs)
/// containing user identity and authorization claims.  
/// The token is typically used for API authentication and session management.
/// </remarks>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Generates a signed JWT for the specified user.
    /// </summary>
    /// <param name="user">
    /// The user whose identity and role claims will populate the token.
    /// </param>
    /// <param name="expiryInMinutes">
    /// Optional custom token lifetime (in minutes).  
    /// If omitted, the implementation’s default expiration will be used.
    /// </param>
    /// <returns>
    /// A string containing the encoded and signed JWT.
    /// </returns>
    string GenerateToken(User user, int? expiryInMinutes = null);
}