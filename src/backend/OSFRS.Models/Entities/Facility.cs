using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OSFRS.Models.Entities;

public class Facility
{
    public int Id { get; set; } // Primary Key

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    [Range(1, 1000)]
    public int Capacity { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Available";
    // Available - Can be reserved
    // Under Maintenance - Temporary unavailable due to maintenance
    // Unavailable - Permanently or manually disabled by admin

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public ICollection<Reservation>? Reservations { get; set; }

    [JsonIgnore]
    public ICollection<MaintenanceRecord>? MaintenanceRecords { get; set; }
}