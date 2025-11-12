using System.ComponentModel.DataAnnotations;

namespace OSFRS.Backend.DTOs;

public record UpdateMaintenanceRecordDto
{
    [MaxLength(200)]
    public string? Description { get; init; }

    public DateTime? StartTime { get; init; }

    public DateTime? EndTime { get; init; }

    [MaxLength(20)]
    public string? Status { get; set; }
}