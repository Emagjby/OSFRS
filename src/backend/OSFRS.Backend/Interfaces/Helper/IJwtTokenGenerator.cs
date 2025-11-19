using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces.Helper;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user, int? expiryInMinutes = null);
}