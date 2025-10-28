using OSFRS.Backend.DTOs;

namespace OSFRS.Backend.Interfaces;

public interface IAuthService
{
    Task<string> LoginAsync(LoginRequestDto dto);
}