using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OSFRS.Backend.Helpers.Auth;

/// <summary>
/// Provides helper methods for extracting authentication-related data
/// from the current <see cref="ClaimsPrincipal"/>.
/// </summary>
public static class UserContextHelper
{
    /// <summary>
    /// Extracts the authenticated user's numeric identifier from JWT claims.
    /// </summary>
    /// <param name="user">
    /// The current authenticated <see cref="ClaimsPrincipal"/> instance
    /// containing JWT and identity claims.
    /// </param>
    /// <returns>
    /// The parsed <c>int</c> user ID if present, otherwise <c>null</c>.
    ///
    /// The helper checks:
    /// <list type="bullet">
    ///     <item><description><c>sub</c> (JWT standard subject claim)</description></item>
    ///     <item><description><see cref="ClaimTypes.NameIdentifier"/></description></item>
    /// </list>
    /// If neither exists or cannot be parsed as an integer, the method returns <c>null</c>.
    /// </returns>
    public static int? GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(JwtRegisteredClaimNames.Sub)
            ?? user.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null) return null;

        return int.TryParse(userIdClaim.Value, out var userId) ? userId : null;
    }
}