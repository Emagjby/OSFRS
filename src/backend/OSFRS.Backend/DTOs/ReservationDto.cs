using System.ComponentModel.DataAnnotations;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.DTOs;

public class ReservationDto
{
    [Required]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    public int FacilityId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}