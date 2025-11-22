using OSFRS.Backend.Interfaces.Base;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces.Repository;

/// <summary>
/// Provides data-access operations related to reservation entities,
/// including availability checks, conflict detection, and range-based queries.
/// </summary>
public interface IReservationRepository : IBaseRepository<Reservation>
{
    /// <summary>
    /// Retrieves all reservations made by a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user whose reservations should be returned.</param>
    /// <returns>
    /// A collection of <see cref="Reservation"/> associated with the specified user.
    /// </returns>
    Task<IEnumerable<Reservation>> GetByUserAsync(int userId);

    /// <summary>
    /// Updates the status of a reservation.
    /// </summary>
    /// <param name="id">The reservation ID.</param>
    /// <param name="status">The new status value.</param>
    /// <returns>
    /// The updated <see cref="Reservation"/> entity, or <c>null</c> if no match exists.
    /// </returns>
    Task<Reservation?> UpdateStatusAsync(int id, string status);

    /// <summary>
    /// Determines whether a facility has an available time slot for the specified range.
    /// </summary>
    /// <param name="start">The requested start time.</param>
    /// <param name="end">The requested end time.</param>
    /// <param name="facilityId">The facility ID.</param>
    /// <returns>
    /// <c>true</c> if the slot is free; otherwise <c>false</c>.
    /// </returns>
    Task<bool> IsSlotAvailableAsync(DateTime start, DateTime end, int facilityId);

    /// <summary>
    /// Retrieves reservations for a facility within an optional time range.
    /// </summary>
    /// <param name="facilityId">The target facility ID.</param>
    /// <param name="start">Optional start of the range (UTC).</param>
    /// <param name="end">Optional end of the range (UTC).</param>
    /// <returns>
    /// A collection of <see cref="Reservation"/> matching the range.
    /// </returns>
    Task<IEnumerable<Reservation>> GetByFacilityAndRangeAsync(
        int facilityId,
        DateTime? start = null,
        DateTime? end = null
    );

    /// <summary>
    /// Searches reservations using optional filters.
    /// </summary>
    /// <param name="userId">Filter by user ID.</param>
    /// <param name="facilityId">Filter by facility ID.</param>
    /// <param name="start">Filter by minimum start time.</param>
    /// <param name="end">Filter by maximum end time.</param>
    /// <returns>
    /// A collection of <see cref="Reservation"/> matching the filters.
    /// </returns>
    Task<IEnumerable<Reservation>> SearchAsync(
        int? userId = null,
        int? facilityId = null,
        DateTime? start = null,
        DateTime? end = null
    );

    /// <summary>
    /// Checks whether a reservation time range overlaps with an existing reservation.
    /// </summary>
    /// <param name="facilityId">The facility ID.</param>
    /// <param name="start">The proposed start time.</param>
    /// <param name="end">The proposed end time.</param>
    /// <param name="excludeReservationId">
    /// An existing reservation ID to exclude from collision detection
    /// (useful for updates).
    /// </param>
    /// <returns>
    /// <c>true</c> if a conflict exists; otherwise <c>false</c>.
    /// </returns>
    Task<bool> HasConflictAsync(int facilityId, DateTime start, DateTime end, int excludeReservationId);
}