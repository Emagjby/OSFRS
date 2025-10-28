using OSFRS.Backend.DTOs;

namespace OSFRS.Backend.Interfaces;

public interface IProfileService
{
    Task<UserProfileDto> GetProfileAsync(int userId);
    Task UpdateProfileAsync(int userId, UpdatedProfileDto dto);
}