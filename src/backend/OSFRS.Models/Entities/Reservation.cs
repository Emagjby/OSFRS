using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSFRS.Models.Entities;

public class Reservation
{
    public int Id { get; set; } // Primary Key

    [Required]
    public int UserId { get; set; } // Foreign Key
    
    public int FacilityId { get; set; } // Abstract for now will be Foreign Key

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
}