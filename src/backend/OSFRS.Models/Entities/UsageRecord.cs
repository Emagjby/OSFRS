using System.ComponentModel.DataAnnotations;

namespace OSFRS.Models.Entities;

public class UsageRecord
{
    public int Id { get; set; } // Primary Key

    [Required]
    [MaxLength(50)]
    public string EventType { get; set; } = string.Empty;
    // examples: ReservationCreated, ReservationCancelled, etc.

    public int? UserId { get; set; }
    public User? User { get; set; }

    public int? FacilityId { get; set; }
    public Facility? Facility { get; set; }

    [Required]
    public DateTime Timestamp { get; set; }

    public string? AggregatedData { get; set; } 
}