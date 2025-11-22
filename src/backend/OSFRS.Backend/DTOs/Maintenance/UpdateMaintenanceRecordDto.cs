namespace OSFRS.Backend.DTOs.Maintenance;

/// <summary>
/// Represents a request to update an existing maintenance record.
/// All properties are optional; only provided values will be applied.
/// </summary>
public record UpdateMaintenanceRecordDto
{
    /// <summary>
    /// Updated description of the maintenance work.
    /// If null, the previous description is preserved.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// New UTC start time for the maintenance window.
    /// If provided, must not be in the past and must remain earlier than <see cref="EndTime"/>.
    /// </summary>
    public DateTime? StartTime { get; init; }

    /// <summary>
    /// New UTC end time for the maintenance window.
    /// If provided, must be after <see cref="StartTime"/>.
    /// </summary>
    public DateTime? EndTime { get; init; }

    /// <summary>
    /// Updated maintenance status.
    /// Typical values include "Scheduled", "Active", or "Completed".
    /// </summary>
    public string? Status { get; set; }
}