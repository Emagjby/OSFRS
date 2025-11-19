using OSFRS.Backend.DTOs.Auth;

namespace OSFRS.Backend.Interfaces.Service;

public interface IProfileService
{
    Task<UserProfileDto> GetProfileAsync(int userId);
    Task UpdateProfileAsync(int userId, UpdatedProfileDto dto);
}