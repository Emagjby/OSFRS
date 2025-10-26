using System.ComponentModel.DataAnnotations;

namespace OSFRS.Backend.DTOs;

public class LoginRequestDto
{
    [Required]
    public string UsernameOrEmail { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}