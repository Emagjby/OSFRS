using System.ComponentModel.DataAnnotations;

namespace OSFRS.Backend.DTOs;

public record CreateMaintenanceRecordDto
{
    public int FacilityId { get; init; }

    [Required]
    [MaxLength(200)]
    public string Description { get; init; } = string.Empty;

    [Required]
    public DateTime StartTime { get; init; }

    [Required]
    public DateTime EndTime { get; init; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Scheduled";
}