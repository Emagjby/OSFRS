using System.ComponentModel.DataAnnotations;

namespace OSFRS.Backend.DTOs;

public class UpdatedProfileDto
{
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Username { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    // Optional
    public string? Password { get; set; }
}