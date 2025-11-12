using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSFRS.Models.Entities;

public class MaintenanceRecord
{
    public int Id { get; set; } // Primary Key

    public int FacilityId { get; set; } // Foreign Key
    public Facility Facility { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Scheduled";
    // Scheduled - Created but not started yet
    // InProgress - Maintenance currently being performed
    // Completed - Finished, facility can be marked available again

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}