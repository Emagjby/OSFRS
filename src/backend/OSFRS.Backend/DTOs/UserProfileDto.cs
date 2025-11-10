using System.ComponentModel.DataAnnotations;

namespace OSFRS.Backend.DTOs;

public record UserProfileDto
{
    [Required]
    public int Id { get; init; }

    [Required]
    public string Name { get; init; } = null!;

    [Required]
    public string Username { get; init; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = null!;

    [Required]
    public string Role { get; init; } = null!;

    // Info only
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}