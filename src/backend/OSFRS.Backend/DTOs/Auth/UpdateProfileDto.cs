namespace OSFRS.Backend.DTOs.Auth;

public record UpdatedProfileDto
{
    public string Name { get; init; } = null!;

    public string Username { get; init; } = null!;

    public string Email { get; init; } = null!;

    // Optional
    public string? Password { get; init; }
}