namespace OSFRS.Backend.DTOs.Auth;

/// <summary>
/// Represents the data required to register a new user account.
/// Used during the initial signup process.
/// </summary>
public record UserRegistrationDto
{
    /// <summary>
    /// The full name of the user being registered.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// The chosen username for the new account.
    /// Must be unique and follow the system's username rules.
    /// </summary>
    public string Username { get; init; } = null!;

    /// <summary>
    /// The email address of the new user.
    /// Must be unique and in valid email format.
    /// </summary>
    public string Email { get; init; } = null!;

    /// <summary>
    /// The plaintext password for the new account.
    /// Must meet system password strength requirements.
    /// </summary>
    public string Password { get; init; } = null!;
}