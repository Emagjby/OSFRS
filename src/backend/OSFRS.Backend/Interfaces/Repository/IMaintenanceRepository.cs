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
    /// Retrieves maintenance records optionally filtered by status and/or facility.
    /// Filtering is performed at the database level.
    /// </summary>
    /// <param name="status">
    /// Optional maintenance status (e.g., <c>Scheduled</c>, <c>InProgress</c>, <c>Completed</c>, <c>Cancelled</c>).
    /// If <c>null</c>, no status filtering is applied.
    /// </param>
    /// <param name="facilityId">
    /// Optional facility identifier. If provided, only maintenance for the specified
    /// facility will be returned.
    /// </param>
    /// <returns>
    /// A collection of <see cref="MaintenanceRecord"/> objects matching the applied filters.
    /// </returns>
    Task<IEnumerable<MaintenanceRecord>> QueryAsync(string? status = null, int? facilityId = null);

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
