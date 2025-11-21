using Microsoft.AspNetCore.Http.HttpResults;
using OSFRS.Backend.DTOs.Maintenance;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Backend.Validators.Base;

namespace OSFRS.Backend.Validators.Maintenance;

public class CreateMaintenanceValidator : BaseValidator, IValidator<CreateMaintenanceRecordDto>
{
    private readonly IFacilityRepository _facility;
    private readonly IMaintenanceRepository _maintenance;

    public CreateMaintenanceValidator(
        IFacilityRepository facility,
        IMaintenanceRepository maintenance)
    {
        _facility = facility;
        _maintenance = maintenance;
    }

    public async Task ValidateAsync(CreateMaintenanceRecordDto dto)
    {
        EnsureValidId(dto.FacilityId, "Invalid facility ID.");

        var facility = await _facility.GetByIdAsync(dto.FacilityId);
        EnsureFound(facility, "Facility not found.");

        EnsureValidTimeRange(dto.StartTime, dto.EndTime, "StartTime must be before EndTime.");
        EnsureNotPast(dto.StartTime, "Maintenance cannot start in the past.");

        var existing = await _maintenance.GetByFacilityAsync(dto.FacilityId);

        bool overlaps = existing.Any(m =>
            dto.StartTime < m.EndTime && dto.EndTime > m.StartTime);

        if (overlaps)
            Conflict("Maintenance period overlaps another maintenance window for this facility.");
    }
}