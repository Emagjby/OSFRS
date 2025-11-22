using OSFRS.Backend.DTOs.Maintenance;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Backend.Validators.Base;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Validators.Maintenance;

/// <summary>
/// Validates updates to an existing <see cref="MaintenanceRecord"/>.
/// Ensures the record exists, the time window is valid, the update is not applied to past maintenance,
/// and the new time window does not overlap with other maintenance operations.
/// </summary>
public class UpdateMaintenanceValidator :
    BaseValidator,
    IUpdateValidator<UpdateMaintenanceRecordDto, MaintenanceRecord>
{
    private readonly IMaintenanceRepository _repo;

    /// <summary>
    /// Creates an instance of <see cref="UpdateMaintenanceValidator"/>.
    /// </summary>
    /// <param name="repo">Repository used to check for overlapping maintenance entries.</param>
    public UpdateMaintenanceValidator(IMaintenanceRepository repo)
    {
        _repo = repo;
    }

    /// <summary>
    /// Validates the incoming update request against the existing maintenance record.
    /// </summary>
    /// <param name="dto">Update request containing modified maintenance fields.</param>
    /// <param name="existing">The existing maintenance record fetched from storage.</param>
    /// <returns>A completed task if validation succeeds.</returns>
    public async Task ValidateAsync(UpdateMaintenanceRecordDto dto, MaintenanceRecord existing)
    {
        EnsureFound(existing, "Maintenance record not found.");

        var now = DateTime.UtcNow;

        if (existing.EndTime < now)
            Forbidden("Past maintenance records cannot be modified.");

        var newStart = dto.StartTime ?? existing.StartTime;
        var newEnd = dto.EndTime ?? existing.EndTime;

        EnsureValidTimeRange(newStart, newEnd, "StartTime must be before EndTime.");

        if (dto.StartTime.HasValue)
            EnsureNotPast(dto.StartTime.Value, "Maintenance cannot start in the past.");

        var others = await _repo.GetByFacilityAsync(existing.FacilityId);

        bool overlaps = others
            .Where(m => m.Id != existing.Id)
            .Any(m => newStart < m.EndTime && newEnd > m.StartTime);

        if (overlaps)
            Conflict("Updated maintenance window overlaps another maintenance entry.");
    }
}