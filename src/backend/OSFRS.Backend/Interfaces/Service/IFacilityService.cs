using OSFRS.Backend.DTOs.Facilities;
using OSFRS.Backend.Interfaces.Base;

namespace OSFRS.Backend.Interfaces.Service;

/// <summary>
/// Provides operations for managing facilities, including creation,
/// updates, deletion, and availability control.
/// </summary>
public interface IFacilityService : ICrudService<CreateFacilityDto, UpdateFacilityDto, FacilityDto>
{
    /// <summary>
    /// Updates the availability status of a facility.
    /// </summary>
    /// <param name="facilityId">The identifier of the facility to update.</param>
    /// <param name="isAvailable">The new availability state.</param>
    /// <returns>
    /// True if the availability was updated successfully;
    /// false if the facility does not exist.
    /// </returns>
    Task<bool> UpdateAvailabilityAsync(int facilityId, bool isAvailable);

    /// <summary>
    /// Determines whether the specified facility is currently available.
    /// </summary>
    /// <param name="facilityId">The identifier of the facility to check.</param>
    /// <returns>
    /// True if the facility is available; otherwise false.
    /// </returns>
    Task<bool> IsFacilityAvailableAsync(int facilityId);
}