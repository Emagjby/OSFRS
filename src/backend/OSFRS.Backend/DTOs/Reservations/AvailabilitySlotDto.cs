using System.ComponentModel.DataAnnotations;

namespace OSFRS.Backend.DTOs;

public record AvailabilitySlotDto
{
    [Required]
    public int Id { get; init; }

    public int FacilityId { get; init; }

    [Required]
    public int UserId { get; init; }

    [Required]
    public DateTime StartTime { get; init; }

    [Required]
    public DateTime EndTime { get; init; }

    [Required]
    [MaxLength(20)]
    public string Status { get; init; } = null!;
}