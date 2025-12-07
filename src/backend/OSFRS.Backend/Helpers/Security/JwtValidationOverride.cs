namespace OSFRS.Backend.Helpers.Security;

public class JwtValidationOverride
{
    public Action<Microsoft.IdentityModel.Tokens.TokenValidationParameters>? Override { get; set; }
}