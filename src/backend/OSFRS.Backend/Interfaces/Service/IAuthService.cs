using OSFRS.Backend.DTOs.Auth;

namespace OSFRS.Backend.Interfaces.Service;

public interface IAuthService
{
    Task RegisterUserAsync(UserRegistrationDto dto);
    Task<string> LoginAsync(LoginRequestDto dto);
}