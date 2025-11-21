using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Backend.Validators.Base;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Validators.Facilities;

public class FacilityAvailabilityValidator : BaseValidator
{
    private readonly IMaintenanceRepository _repo;

    public FacilityAvailabilityValidator(IMaintenanceRepository repo)
    {
        _repo = repo;
    }

    public async Task ValidateAsync(Facility facility, bool newAvailability)
    {
        EnsureFound(facility, "Facility not found.");

        var now = DateTime.UtcNow;

        var activeMaintenance = await _repo.GetByFacilityAsync(facility.Id);

        bool isUnderMaintenance = activeMaintenance.Any(m =>
            now >= m.StartTime && now <= m.EndTime);

        if (newAvailability && isUnderMaintenance)
            Forbidden("Facility cannot be marked Available during active maintenance.");
    }
}