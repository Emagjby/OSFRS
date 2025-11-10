using System.ComponentModel.DataAnnotations;

namespace OSFRS.Backend.DTOs;

public record LoginRequestDto
{
    [Required]
    public string UsernameOrEmail { get; init; } = null!;

    [Required]
    public string Password { get; init; } = null!;
}