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
    public string? Name { get; init; }

    /// <summary>
    /// The username associated with the user.
    /// </summary>
    public string? Username { get; init; }

    /// <summary>
    /// The user's email address.
    /// </summary>
    public string? Email { get; init; }
}
