using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Backend.Validators.Base;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Validators.Facilities;

/// <summary>
/// Validates whether a facility's availability status can be changed.
/// Prevents enabling availability during active maintenance windows.
/// </summary>
public class FacilityAvailabilityValidator : BaseValidator
{
    private readonly IMaintenanceRepository _repo;

    /// <summary>
    /// Initializes a new instance of the <see cref="FacilityAvailabilityValidator"/>.
    /// </summary>
    /// <param name="repo">The maintenance repository used to check active maintenance windows.</param>
    public FacilityAvailabilityValidator(IMaintenanceRepository repo)
    {
        _repo = repo;
    }

    /// <summary>
    /// Validates whether the specified <see cref="Facility"/> can transition to the requested availability state.
    /// </summary>
    /// <param name="facility">The facility being updated.</param>
    /// <param name="newAvailability">The requested availability state.</param>
    /// <returns>A completed task if validation succeeds.</returns>
    public async Task ValidateAsync(Facility facility, bool newAvailability)
    {
        EnsureFound(facility, "Facility not found.");

        var now = DateTime.UtcNow;
        var activeMaintenance = await _repo.GetByFacilityAsync(facility.Id);

        bool isUnderMaintenance = activeMaintenance.Any(m =>
            now >= m.StartTime && now <= m.EndTime
        );

        if (newAvailability && isUnderMaintenance)
        {
            Forbidden("Facility cannot be marked Available during active maintenance.");
        }
    }
}