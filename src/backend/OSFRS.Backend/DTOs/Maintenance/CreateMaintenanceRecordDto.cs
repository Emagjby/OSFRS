namespace OSFRS.Backend.DTOs.Maintenance;

/// <summary>
/// Represents a request to create a new maintenance record for a facility.
/// Defines the maintenance window, description, and initial status.
/// </summary>
public record CreateMaintenanceRecordDto
{
    /// <summary>
    /// The ID of the facility for which maintenance is being scheduled.
    /// Must refer to an existing facility.
    /// </summary>
    public int FacilityId { get; init; }

    /// <summary>
    /// A human-readable description of the maintenance work.
    /// Optional but recommended for reporting and auditing.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// The UTC timestamp indicating when the maintenance period begins.
    /// Must not be in the past.
    /// </summary>
    public DateTime StartTime { get; init; }

    /// <summary>
    /// The UTC timestamp indicating when the maintenance period ends.
    /// Must be after <see cref="StartTime"/>.
    /// </summary>
    public DateTime EndTime { get; init; }

    /// <summary>
    /// The initial status of the maintenance record.
    /// Defaults to "Scheduled".
    /// </summary>
    public string Status { get; set; } = "Scheduled";
}