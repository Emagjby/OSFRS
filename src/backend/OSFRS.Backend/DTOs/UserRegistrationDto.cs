using System.ComponentModel.DataAnnotations;

namespace OSFRS.Backend.DTOs;

public class UserRegistrationDto
{
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Username { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = null!;
}