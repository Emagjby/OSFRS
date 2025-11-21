using OSFRS.Backend.DTOs.Maintenance;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Backend.Validators.Base;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Validators.Maintenance;

public class UpdateMaintenanceValidator :
    BaseValidator,
    IUpdateValidator<UpdateMaintenanceRecordDto, MaintenanceRecord>
{
    private readonly IMaintenanceRepository _repo;

    public UpdateMaintenanceValidator(IMaintenanceRepository repo)
    {
        _repo = repo;
    }

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

        var other = await _repo.GetByFacilityAsync(existing.FacilityId);

        bool overlaps = other
            .Where(m => m.Id != existing.Id)
            .Any(m => newStart < m.EndTime && newEnd > m.StartTime);

        if (overlaps)
            Conflict("Updated maintenance window overlaps another maintenance entry.");
    }
}