using System.ComponentModel.DataAnnotations;

namespace OSFRS.Backend.DTOs;

public record UpdatedProfileDto
{
    public string Name { get; init; } = null!;

    public string Username { get; init; } = null!;

    [EmailAddress]
    public string Email { get; init; } = null!;

    // Optional
    public string? Password { get; init; }
}