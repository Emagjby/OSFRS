using System.ComponentModel.DataAnnotations;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.DTOs;

public record ReservationDto
{
    [Required]
    public int Id { get; init; }

    [Required]
    public int UserId { get; init; }

    public int FacilityId { get; init; }

    [Required]
    public DateTime StartTime { get; init; }

    [Required]
    public DateTime EndTime { get; init; }

    [Required]
    [MaxLength(20)]
    public string Status { get; init; } = null!;

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}