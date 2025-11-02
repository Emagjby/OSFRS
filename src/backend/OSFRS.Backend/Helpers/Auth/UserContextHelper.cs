using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OSFRS.Backend.Helpers.Auth;

public static class UserContextHelper
{
    public static int? GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(JwtRegisteredClaimNames.Sub)
            ?? user.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null) return null;

        return int.TryParse(userIdClaim.Value, out var userId) ? userId : null;
    }
}