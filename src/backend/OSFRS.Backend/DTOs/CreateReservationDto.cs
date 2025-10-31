using System.ComponentModel.DataAnnotations;

namespace OSFRS.Backend.DTOs;

public class CreateReservationDto
{
    [Required]
    public int UserId { get; set; }

    public int FacilityId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }
    
    [Required]
    public DateTime EndTime { get; set; }
}