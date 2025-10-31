using System.ComponentModel.DataAnnotations;

namespace OSFRS.Backend.DTOs;

public class AvailabilitySlotDto
{
    [Required]
    public int Id { get; set; }

    public int FacilityId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = null!;
}