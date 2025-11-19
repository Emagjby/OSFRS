using OSFRS.Backend.DTOs;

namespace OSFRS.Backend.Interfaces;

public interface IAuthService
{
    Task RegisterUserAsync(UserRegistrationDto dto);
    Task<string> LoginAsync(LoginRequestDto dto);
}