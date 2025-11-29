using OSFRS.Backend.DTOs.Maintenance;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces.Service;

/// <summary>
/// Provides operations for scheduling, updating, deleting, and retrieving
/// maintenance records, as well as synchronizing facility statuses.
/// </summary>
public interface IMaintenanceService
{
    /// <summary>
    /// Retrieves maintenance records using optional filtering by status and/or facility.
    /// </summary>
    /// <param name="status">
    /// Optional maintenance status filter (e.g. <c>Scheduled</c>, <c>InProgress</c>,
    /// <c>Completed</c>, <c>Cancelled</c>). If omitted, all statuses are included.
    /// </param>
    /// <param name="facilityId">
    /// Optional facility ID to limit results. If <c>null</c>, records for all facilities
    /// are returned.
    /// </param>
    /// <returns>
    /// A collection of <see cref="MaintenanceRecord"/> instances that match the filters.
    /// </returns>
    Task<IEnumerable<MaintenanceRecord>> GetFilteredMaintenanceAsync(
        string? status = null,
        int? facilityId = null
    );

    /// <summary>
    /// Schedules a new maintenance record for a facility.
    /// </summary>
    /// <param name="dto">The maintenance record details to create.</param>
    /// <returns>
    /// The newly created <see cref="MaintenanceRecord"/>.
    /// </returns>
    Task<MaintenanceRecord> ScheduleMaintenanceAsync(CreateMaintenanceRecordDto dto);

    /// <summary>
    /// Updates an existing maintenance record.
    /// </summary>
    /// <param name="id">The identifier of the maintenance record to update.</param>
    /// <param name="dto">The updated maintenance details.</param>
    /// <returns>
    /// The updated <see cref="MaintenanceRecord"/>, or null if the record was not found.
    /// </returns>
    Task<MaintenanceRecord?> UpdateMaintenanceAsync(int id, UpdateMaintenanceRecordDto dto);

    /// <summary>
    /// Deletes a maintenance record.
    /// </summary>
    /// <param name="id">The identifier of the maintenance record to delete.</param>
    /// <returns>
    /// True if deletion succeeded; false if the record does not exist.
    /// </returns>
    Task<bool> DeleteMaintenanceAsync(int id);

    /// <summary>
    /// Retrieves all maintenance records for a specific facility.
    /// </summary>
    /// <param name="facilityId">The facility identifier.</param>
    /// <returns>
    /// A collection of <see cref="MaintenanceRecord"/> items for the facility.
    /// </returns>
    Task<IEnumerable<MaintenanceRecord>> GetMaintenanceByFacilityAsync(int facilityId);

    /// <summary>
    /// Retrieves all upcoming maintenance records relative to the current UTC time.
    /// </summary>
    /// <returns>
    /// A collection of future <see cref="MaintenanceRecord"/> entries.
    /// </returns>
    Task<IEnumerable<MaintenanceRecord>> GetUpcomingMaintenanceAsync();

    /// <summary>
    /// Synchronizes maintenance records and facility availability based on current time.
    /// </summary>
    Task SyncStatusesAsync();
}
