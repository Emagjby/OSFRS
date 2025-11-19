using System.ComponentModel.DataAnnotations;

namespace OSFRS.Backend.DTOs;

public record CreateReservationDto
{
    public int FacilityId { get; init; }

    [Required]
    public DateTime StartTime { get; init; }
    
    [Required]
    public DateTime EndTime { get; init; }
}