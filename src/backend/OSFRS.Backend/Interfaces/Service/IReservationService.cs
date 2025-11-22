using OSFRS.Backend.DTOs.Reports;
using OSFRS.Backend.DTOs.Reservations;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces.Service;

/// <summary>
/// Provides operations for creating, updating, querying, and managing facility reservations.
/// </summary>
public interface IReservationService
{
    /// <summary>
    /// Retrieves reservations for a given facility within an optional date range.
    /// </summary>
    /// <param name="facilityId">The ID of the facility.</param>
    /// <param name="start">Optional start of the date range (UTC).</param>
    /// <param name="end">Optional end of the date range (UTC).</param>
    /// <returns>
    /// A collection of <see cref="Reservation"/> instances matching the criteria.
    /// </returns>
    Task<IEnumerable<Reservation>> GetReservationsAsync(int facilityId, DateTime? start = null, DateTime? end = null);

    /// <summary>
    /// Retrieves the availability calendar for a specific facility on a given day.
    /// </summary>
    /// <param name="facilityId">The ID of the facility.</param>
    /// <param name="date">
    /// The target date (UTC).  
    /// If null, the current UTC date is used.
    /// </param>
    /// <returns>
    /// A list of availability slots describing existing reservations.
    /// </returns>
    Task<IEnumerable<AvailabilitySlotDto>> GetAvailabilityCalendarAsync(int facilityId, DateTime? date = null);

    /// <summary>
    /// Searches reservation records with flexible filters.
    /// </summary>
    /// <param name="userId">Optional user ID to filter by.</param>
    /// <param name="facilityId">Optional facility ID to filter by.</param>
    /// <param name="start">Optional start of the date range (UTC).</param>
    /// <param name="end">Optional end of the date range (UTC).</param>
    /// <returns>
    /// A collection of <see cref="Reservation"/> objects matching the search parameters.
    /// </returns>
    Task<IEnumerable<Reservation>> SearchReservationAsync(
        int? userId = null,
        int? facilityId = null,
        DateTime? start = null,
        DateTime? end = null
    );

    /// <summary>
    /// Creates a new reservation for a user.
    /// </summary>
    /// <param name="dto">The reservation details.</param>
    /// <param name="userId">The ID of the user making the reservation.</param>
    /// <returns>
    /// The newly created <see cref="Reservation"/> record.
    /// </returns>
    Task<Reservation> CreateReservationAsync(CreateReservationDto dto, int userId);

    /// <summary>
    /// Updates an existing reservation if the user has permission.
    /// </summary>
    /// <param name="id">The reservation ID.</param>
    /// <param name="dto">The updated reservation details.</param>
    /// <param name="userId">The ID of the user performing the update.</param>
    /// <returns>
    /// The updated <see cref="Reservation"/> instance.
    /// </returns>
    Task<Reservation> UpdateReservationAsync(int id, UpdateReservationDto dto, int userId);

    /// <summary>
    /// Cancels a reservation owned by the specified user.
    /// </summary>
    /// <param name="id">The reservation ID.</param>
    /// <param name="userId">The ID of the user requesting cancellation.</param>
    Task CancelReservationAsync(int id, int userId);

    /// <summary>
    /// Retrieves all reservations in the system.  
    /// Restricted to administrative use.
    /// </summary>
    /// <returns>
    /// A list of <see cref="Reservation"/> records.
    /// </returns>
    Task<IEnumerable<Reservation>> GetAllReservationsAsync();

    /// <summary>
    /// Deletes a reservation as an administrator.
    /// </summary>
    /// <param name="id">The reservation ID.</param>
    /// <param name="adminId">The ID of the administrator performing the deletion.</param>
    Task DeleteReservationAsync(int id, int adminId);

    /// <summary>
    /// Updates a reservation with administrative privileges,
    /// allowing status changes and time conflict overrides.
    /// </summary>
    /// <param name="id">The reservation ID.</param>
    /// <param name="dto">The updated details.</param>
    /// <param name="adminId">The ID of the admin user.</param>
    /// <returns>
    /// The updated <see cref="Reservation"/> instance.
    /// </returns>
    Task<Reservation> AdminUpdateReservationAsync(int id, UpdateReservationDto dto, int adminId);
}