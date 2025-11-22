using OSFRS.Backend.DTOs.Auth;

namespace OSFRS.Backend.Interfaces.Service;

/// <summary>
/// Provides authentication services including user registration
/// and credential-based login.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user in the system.
    /// Validation is applied before persistence.
    /// </summary>
    /// <param name="dto">The registration details including name, username, email, and password.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RegisterUserAsync(UserRegistrationDto dto);

    /// <summary>
    /// Authenticates a user using username/email and password
    /// and returns a JWT token upon success.
    /// </summary>
    /// <param name="dto">The login request containing credentials.</param>
    /// <returns>
    /// A JWT token string representing the authenticated session.
    /// </returns>
    Task<string> LoginAsync(LoginRequestDto dto);
}