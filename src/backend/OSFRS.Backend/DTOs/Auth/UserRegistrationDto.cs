using System.ComponentModel.DataAnnotations;

namespace OSFRS.Backend.DTOs;

public record UserRegistrationDto
{
    [Required]
    public string Name { get; init; } = null!;

    [Required]
    public string Username { get; init; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = null!;

    [Required]
    [MinLength(8)]
    public string Password { get; init; } = null!;
}