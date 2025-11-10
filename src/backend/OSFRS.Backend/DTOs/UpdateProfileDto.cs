using System.ComponentModel.DataAnnotations;

namespace OSFRS.Backend.DTOs;

public record UpdatedProfileDto
{
    [Required]
    public string Name { get; init; } = null!;

    [Required]
    public string Username { get; init; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = null!;

    // Optional
    public string? Password { get; init; }
}