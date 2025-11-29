using Microsoft.EntityFrameworkCore;
using OSFRS.Backend.Data;
using OSFRS.Backend.Exceptions;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Repositories;

/// <summary>
/// Repository responsible for managing reservation data, including
/// queries by user, facility, time ranges, conflict detection, and status updates.
/// </summary>
public class ReservationRepository : BaseRepository<Reservation>, IReservationRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReservationRepository"/> class.
    /// </summary>
    /// <param name="context">The database context used for data access.</param>
    /// <param name="logger">Logger for diagnostic and audit information.</param>
    public ReservationRepository(
        OSFRSDbContext context,
        IAppLogger<BaseRepository<Reservation>> logger
    )
        : base(context, logger) { }

    /// <summary>
    /// Retrieves all reservations associated with the specified user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A collection of reservations.</returns>
    public async Task<IEnumerable<Reservation>> GetByUserAsync(int userId)
    {
        return await _dbSet.Where(reservation => reservation.UserId == userId).ToListAsync();
    }

    /// <summary>
    /// Retrieves all reservations including the related user entity.
    /// </summary>
    /// <returns>A collection of enriched reservation objects.</returns>
    public async Task<IEnumerable<Reservation>> GetAllWithUserAsync()
    {
        return await _dbSet.Include(r => r.User).ToListAsync();
    }

    /// <summary>
    /// Retrieves a reservation by its identifier, including the associated user.
    /// </summary>
    /// <param name="id">The reservation identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The reservation, or null if not found.</returns>
    public override async Task<Reservation?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id);
    }

    /// <summary>
    /// Determines whether a reservation slot is free for the specified time range and facility.
    /// </summary>
    /// <param name="start">Start time of the reservation.</param>
    /// <param name="end">End time of the reservation.</param>
    /// <param name="facilityId">Facility identifier.</param>
    /// <returns>True if the slot is available; otherwise false.</returns>
    public async Task<bool> IsSlotAvailableAsync(DateTime start, DateTime end, int facilityId)
    {
        return !await _dbSet.AnyAsync(r =>
            r.FacilityId == facilityId
            && r.Status != "Cancelled"
            && start < r.EndTime
            && end > r.StartTime
        );
    }

    /// <summary>
    /// Retrieves reservations by facility and optional time range.
    /// </summary>
    /// <param name="facilityId">The facility identifier.</param>
    /// <param name="start">Optional range start.</param>
    /// <param name="end">Optional range end.</param>
    /// <returns>A collection of matching reservations.</returns>
    public async Task<IEnumerable<Reservation>> GetByFacilityAndRangeAsync(
        int facilityId,
        DateTime? start = null,
        DateTime? end = null
    )
    {
        var query = _dbSet.Where(r => r.FacilityId == facilityId);

        if (start.HasValue && end.HasValue)
        {
            var rangeStart = start.Value;
            var rangeEnd = end.Value;

            query = query.Where(r => r.StartTime < rangeEnd && r.EndTime > rangeStart);
        }
        else
        {
            if (start.HasValue)
                query = query.Where(r => r.EndTime > start.Value);

            if (end.HasValue)
                query = query.Where(r => r.StartTime < end.Value);
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// Searches for reservations matching the provided filter criteria.
    /// </summary>
    /// <param name="userId">Optional user identifier.</param>
    /// <param name="facilityId">Optional facility identifier.</param>
    /// <param name="start">Optional start time filter.</param>
    /// <param name="end">Optional end time filter.</param>
    /// <returns>A collection of matching reservations.</returns>
    public async Task<IEnumerable<Reservation>> SearchAsync(
        int? userId = null,
        int? facilityId = null,
        DateTime? start = null,
        DateTime? end = null
    )
    {
        var query = _dbSet.AsQueryable();

        if (userId.HasValue)
            query = query.Where(reservation => reservation.UserId == userId);

        if (facilityId.HasValue)
            query = query.Where(reservation => reservation.FacilityId == facilityId);

        if (start.HasValue)
            query = query.Where(reservation => reservation.StartTime >= start);

        if (end.HasValue)
            query = query.Where(reservation => reservation.EndTime <= end);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Updates the status of a reservation.
    /// </summary>
    /// <param name="id">Reservation identifier.</param>
    /// <param name="status">New status value.</param>
    /// <returns>The updated reservation.</returns>
    public async Task<Reservation?> UpdateStatusAsync(int id, string status)
    {
        var reservation =
            await _dbSet.FindAsync(id) ?? throw new NotFoundException("Reservation not found.");

        reservation!.Status = status;
        reservation.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "Reservation {ReservationId} status updated to {Status}",
            id,
            status
        );

        return reservation;
    }

    /// <summary>
    /// Checks whether a reservation time range overlaps with an existing reservation, excluding a specific reservation by ID.
    /// </summary>
    /// <param name="facilityId">Facility identifier.</param>
    /// <param name="start">Proposed start time.</param>
    /// <param name="end">Proposed end time.</param>
    /// <param name="excludeReservationId">Reservation to exclude from conflict detection.</param>
    /// <returns>True if a conflict exists; otherwise false.</returns>
    public async Task<bool> HasConflictAsync(
        int facilityId,
        DateTime start,
        DateTime end,
        int excludeReservationId
    )
    {
        var conflictExists = await _dbSet.AnyAsync(reservation =>
            reservation.FacilityId == facilityId
            && reservation.Id != excludeReservationId
            && (
                (start >= reservation.StartTime && start < reservation.EndTime)
                || (end > reservation.StartTime && end <= reservation.EndTime)
                || (start <= reservation.StartTime && end >= reservation.EndTime)
            )
        );

        if (conflictExists)
        {
            _logger.LogWarning(
                "Conflict detected for facility {FacilityId} between {Start} and {End}.",
                facilityId,
                start,
                end
            );
        }

        return conflictExists;
    }
}
