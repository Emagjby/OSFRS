using System.ComponentModel.DataAnnotations;

namespace OSFRS.Backend.DTOs;

public class UpdateReservationDto
{
    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [MaxLength(20)]
    public string? Status { get; set; }
}