using OSFRS.Backend.DTOs.Maintenance;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Backend.Validators.Base;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Validators.Maintenance;

/// <summary>
/// Validates creation of a new <see cref="MaintenanceRecord"/>.
/// Ensures the facility exists, the time window is valid,
/// and the maintenance period does not overlap with existing records.
/// </summary>
public class CreateMaintenanceValidator : BaseValidator, IValidator<CreateMaintenanceRecordDto>
{
    private readonly IFacilityRepository _facility;
    private readonly IMaintenanceRepository _maintenance;

    /// <summary>
    /// Creates a new instance of <see cref="CreateMaintenanceValidator"/>.
    /// </summary>
    /// <param name="facility">Repository used to validate the existence of the referenced facility.</param>
    /// <param name="maintenance">Repository used to detect maintenance overlaps.</param>
    public CreateMaintenanceValidator(
        IFacilityRepository facility,
        IMaintenanceRepository maintenance)
    {
        _facility = facility;
        _maintenance = maintenance;
    }

    /// <summary>
    /// Validates that the provided maintenance record can be created.
    /// </summary>
    /// <param name="dto">The incoming maintenance creation request.</param>
    /// <returns>A completed task if validation succeeds.</returns>
    public async Task ValidateAsync(CreateMaintenanceRecordDto dto)
    {
        EnsureValidId(dto.FacilityId, "Invalid facility ID.");

        var facility = await _facility.GetByIdAsync(dto.FacilityId);
        EnsureFound(facility, "Facility not found.");

        EnsureValidTimeRange(dto.StartTime, dto.EndTime, "StartTime must be before EndTime.");
        EnsureNotPast(dto.StartTime, "Maintenance cannot start in the past.");

        var existing = await _maintenance.GetByFacilityAsync(dto.FacilityId);

        bool overlaps = existing.Any(m =>
            dto.StartTime < m.EndTime &&
            dto.EndTime > m.StartTime
        );

        if (overlaps)
            Conflict("Maintenance period overlaps another maintenance window for this facility.");
    }
}