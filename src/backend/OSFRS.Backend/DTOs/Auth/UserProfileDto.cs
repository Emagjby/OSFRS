namespace OSFRS.Backend.DTOs.Auth;

/// <summary>
/// Represents the complete public profile information of a user.
/// Returned when fetching a user's own profile or when admins inspect user details.
/// </summary>
public record UserProfileDto
{
    /// <summary>
    /// The unique identifier of the user.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// The user's full name.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// The username associated with the user.
    /// </summary>
    public string Username { get; init; } = null!;

    /// <summary>
    /// The user's email address.
    /// </summary>
    public string Email { get; init; } = null!;

    /// <summary>
    /// The role assigned to the user (e.g., User, Admin).
    /// </summary>
    public string Role { get; init; } = null!;

    /// <summary>
    /// The UTC timestamp when the user account was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The UTC timestamp of the user's most recent update.
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}