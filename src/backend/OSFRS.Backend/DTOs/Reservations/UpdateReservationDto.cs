using System.ComponentModel.DataAnnotations;

namespace OSFRS.Backend.DTOs;

public record UpdateReservationDto
{
    [Required]
    public DateTime StartTime { get; init; }

    [Required]
    public DateTime EndTime { get; init; }

    [MaxLength(20)]
    public string? Status { get; init; }
}