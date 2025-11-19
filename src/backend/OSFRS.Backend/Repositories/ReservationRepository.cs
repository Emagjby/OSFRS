using OSFRS.Backend.Interfaces;
using OSFRS.Models.Entities;
using OSFRS.Backend.Data;
using OSFRS.Backend.Interfaces.Logging;
using Microsoft.EntityFrameworkCore;

namespace OSFRS.Backend.Repositories;

public class ReservationRepository : BaseRepository<Reservation>, IReservationRepository
{
    public ReservationRepository(
        OSFRSDbContext context,
        IAppLogger<BaseRepository<Reservation>> logger
    ) : base(context, logger)
    {
        
    }


    public async Task<IEnumerable<Reservation>> GetByUserAsync(int userId)
    {
        return await _dbSet
            .Where(reservation => reservation.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> GetAllWithUserAsync()
    {
        return await _dbSet
            .Include(r => r.User)
            .ToListAsync();
    }

    public override async Task<Reservation?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<bool> IsSlotAvailableAsync(DateTime start, DateTime end, int facilityId)
    {
        return !await _dbSet.AnyAsync(r =>
            r.FacilityId == facilityId &&
            (
                (start >= r.StartTime && start < r.EndTime) ||
                (end > r.StartTime && end <= r.EndTime) ||
                (start <= r.StartTime && end >= r.EndTime)
            )
        );
    }

    public async Task<IEnumerable<Reservation>> GetByFacilityAndRangeAsync(int facilityId, DateTime? start = null, DateTime? end = null)
    {
       var query = _dbSet.Where(r => r.FacilityId == facilityId);

        if (start.HasValue)
            query = query.Where(r => r.StartTime >= start);

        if (end.HasValue)
            query = query.Where(r => r.EndTime <= end);

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> SearchAsync(int? userId = null, int? facilityId = null, DateTime? start = null, DateTime? end = null)
    {
        var query = _dbSet.AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(reservation => reservation.UserId == userId);
        }

        if (facilityId.HasValue)
        {
            query = query.Where(reservation => reservation.FacilityId == facilityId);
        }

        if (start.HasValue)
        {
            query = query.Where(reservation => reservation.StartTime >= start);
        }

        if (end.HasValue)
        {
            query = query.Where(reservation => reservation.EndTime <= end);
        }

        return await query.ToListAsync();
    }

    public async Task<Reservation?> UpdateStatusAsync(int id, string status)
    {
        var reservation = await _dbSet.FindAsync(id);

        if (reservation == null)
        {
            _logger.LogWarning("Attempted to update status for non-existant reservation {ReservationId}", id);
            throw new InvalidOperationException("Reservation not found.");
        }

        reservation.Status = status;
        reservation.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation("Reservation {ReservationId} status updated to {Status}", id, status);

        return reservation;
    }

    public async Task<bool> HasConflictAsync(int facilityId, DateTime start, DateTime end, int excludeReservationId)
    {
        var conflictExists = await _dbSet.AnyAsync(reservation =>
            reservation.FacilityId == facilityId &&
            reservation.Id != excludeReservationId &&
            (
                (start >= reservation.StartTime && start < reservation.EndTime) ||
                (end > reservation.StartTime && end <= reservation.EndTime) ||
                (start <= reservation.StartTime && end >= reservation.EndTime)
            )
        );

        if (conflictExists)
        {
            _logger.LogWarning("Conflict detected for facility {FacilityId} between {Start} and {End}.", facilityId, start, end);
        }

        return conflictExists;
    }
}