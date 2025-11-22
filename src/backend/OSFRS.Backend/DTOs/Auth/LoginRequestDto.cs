namespace OSFRS.Backend.DTOs.Auth;

/// <summary>
/// Represents the credential payload used during user authentication.
/// The client may provide either a username or an email address,
/// along with the associated password.
/// </summary>
public record LoginRequestDto
{
    /// <summary>
    /// The identifier supplied by the user, interpreted as either a username
    /// or an email during authentication.
    /// </summary>
    public string UsernameOrEmail { get; init; } = null!;

    /// <summary>
    /// The plaintext password submitted for verification.
    /// </summary>
    public string Password { get; init; } = null!;
}