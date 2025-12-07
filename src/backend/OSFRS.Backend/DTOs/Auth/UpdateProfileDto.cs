namespace OSFRS.Backend.DTOs.Auth;

/// <summary>
/// Represents the data required to update a user's profile.
/// All fields are optional and replace
/// the user's existing information upon update.
/// </summary>
public record UpdatedProfileDto
{
    /// <summary>
    /// The updated full name of the user.
    /// Must follow formatting rules defined by validation.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// The updated username for the user.
    /// Must remain unique across the system.
    /// </summary>
    public string? Username { get; init; }

    /// <summary>
    /// The updated email address for the user.
    /// Must be valid and unique across the system.
    /// </summary>
    public string? Email { get; init; }
}
