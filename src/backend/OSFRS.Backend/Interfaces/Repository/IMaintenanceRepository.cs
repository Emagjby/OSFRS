using OSFRS.Backend.Interfaces.Base;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces.Repository;

/// <summary>
/// Provides data access operations for <see cref="MaintenanceRecord"/> entities,
/// including facility-specific queries and upcoming maintenance retrieval.
/// </summary>
/// <remarks>
/// Extends <see cref="IBaseRepository{MaintenanceRecord}"/> with domain-specific
/// querying required by scheduling and status-sync workflows.
/// </remarks>
public interface IMaintenanceRepository : IBaseRepository<MaintenanceRecord>
{
    /// <summary>
    /// Retrieves all maintenance records associated with the specified facility.
    /// </summary>
    /// <param name="facilityId">The unique identifier of the facility.</param>
    /// <returns>
    /// A collection of <see cref="MaintenanceRecord"/> entries linked to the facility.
    /// </returns>
    Task<IEnumerable<MaintenanceRecord>> GetByFacilityAsync(int facilityId);

    /// <summary>
    /// Retrieves all upcoming maintenance records with a start time in the future.
    /// </summary>
    /// <returns>
    /// A collection of upcoming <see cref="MaintenanceRecord"/> entries.
    /// </returns>
    Task<IEnumerable<MaintenanceRecord>> GetUpcomingAsync();
}