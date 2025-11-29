using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OSFRS.Models.Entities;

public class User
{
    public int Id { get; set; } // Primary Key

    [Required]
    [MaxLength(50)]
    [JsonIgnore]
    public string Name { get; set; } = null!;

    [Required]
    [MaxLength(30)]
    [JsonIgnore]
    public string Username { get; set; } = null!;

    [Required]
    [EmailAddress]
    [JsonIgnore]
    public string Email { get; set; } = null!;

    [Required]
    public string PasswordHash { get; set; } = null!;

    [Required]
    public string Role { get; set; } = "User"; // User by default

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
