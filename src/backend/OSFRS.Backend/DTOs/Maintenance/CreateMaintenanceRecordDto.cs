namespace OSFRS.Backend.DTOs.Maintenance;

public record CreateMaintenanceRecordDto
{
    public int FacilityId { get; init; }

    public string Description { get; init; } = string.Empty;

    public DateTime StartTime { get; init; }

    public DateTime EndTime { get; init; }

    public string Status { get; set; } = "Scheduled";
}