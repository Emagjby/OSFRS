using OSFRS.Backend.DTOs;

namespace OSFRS.Backend.Interfaces;

public interface IUserService
{
    Task RegisterUserAsync(UserRegistrationDto dto);
}