namespace OSFRS.Backend.DTOs.Auth;

/// <summary>
/// Represents the data required to update a user's profile.
/// All fields except <see cref="Password"/> are mandatory and replace
/// the user's existing information upon update.
/// </summary>
public record UpdatedProfileDto
{
    /// <summary>
    /// The updated full name of the user.
    /// Must follow formatting rules defined by validation.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// The updated username for the user.
    /// Must remain unique across the system.
    /// </summary>
    public string Username { get; init; } = null!;

    /// <summary>
    /// The updated email address for the user.
    /// Must be valid and unique across the system.
    /// </summary>
    public string Email { get; init; } = null!;

    /// <summary>
    /// An optional new password for the user.
    /// When omitted, the existing password remains unchanged.
    /// </summary>
    public string? Password { get; init; }
}