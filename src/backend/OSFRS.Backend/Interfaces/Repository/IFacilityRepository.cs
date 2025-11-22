using OSFRS.Backend.Interfaces.Base;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces.Repository;

/// <summary>
/// Provides data access functionality for <see cref="Facility"/> entities,
/// including availability checks and status updates.
/// </summary>
/// <remarks>
/// Extends the generic <see cref="IBaseRepository{Facility}"/> with
/// facility-specific operations used by scheduling, reservation,
/// and maintenance workflows.
/// </remarks>
public interface IFacilityRepository : IBaseRepository<Facility>
{
    /// <summary>
    /// Updates the availability status of a facility.
    /// </summary>
    /// <param name="facilityId">The unique facility identifier.</param>
    /// <param name="isAvailable">
    /// True if the facility should be marked available;
    /// false to set it as unavailable.
    /// </param>
    Task UpdateAvailabilityAsync(int facilityId, bool isAvailable);

    /// <summary>
    /// Determines whether a facility is currently marked as available.
    /// </summary>
    /// <param name="facilityId">The facility ID.</param>
    /// <returns>
    /// True if the facility is available for use;
    /// otherwise false.
    /// </returns>
    Task<bool> IsFacilityAvailableAsync(int facilityId);
}