namespace OSFRS.Backend.DTOs.Auth;

public record UserProfileDto
{
    public int Id { get; init; }

    public string Name { get; init; } = null!;

    public string Username { get; init; } = null!;

    public string Email { get; init; } = null!;

    public string Role { get; init; } = null!;

    // Info only
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}