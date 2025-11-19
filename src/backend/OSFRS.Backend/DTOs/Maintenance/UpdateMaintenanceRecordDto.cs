namespace OSFRS.Backend.DTOs.Maintenance;

public record UpdateMaintenanceRecordDto
{
    public string? Description { get; init; }

    public DateTime? StartTime { get; init; }

    public DateTime? EndTime { get; init; }

    public string? Status { get; set; }
}