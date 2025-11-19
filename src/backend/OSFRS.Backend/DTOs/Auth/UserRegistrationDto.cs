namespace OSFRS.Backend.DTOs.Auth;

public record UserRegistrationDto
{
    public string Name { get; init; } = null!;

    public string Username { get; init; } = null!;

    public string Email { get; init; } = null!;

    public string Password { get; init; } = null!;
}