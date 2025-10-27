using System.ComponentModel.DataAnnotations;

namespace OSFRS.Backend.DTOs;

public class UserProfileDto
{
    [Required]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Username { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Role { get; set; } = null!;

    // Info only
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}