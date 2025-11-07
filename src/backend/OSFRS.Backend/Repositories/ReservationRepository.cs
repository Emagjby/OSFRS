using OSFRS.Backend.Interfaces;
using OSFRS.Models.Entities;
using OSFRS.Backend.Data;
using OSFRS.Backend.Interfaces.Logging;
using Microsoft.EntityFrameworkCore;

namespace OSFRS.Backend.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly OSFRSDbContext _context;
    private readonly IAppLogger<ReservationRepository> _logger;

    public ReservationRepository(OSFRSDbContext context, IAppLogger<ReservationRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Reservation?> AddAsync(Reservation reservation)
    {
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Reservation created for UserId {UserId}", reservation.UserId);

        return reservation;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if(reservation != null)
        {
            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Reservation {ReservationId} deleted.", id);

            return true;
        }
        else
        {
            _logger.LogWarning("Attempted to delete non-existent reservation with ID {ReservationId}", id);
            return false;
        }
    }

    public async Task<IEnumerable<Reservation>> GetAllAsync()
    {
        return await _context.Reservations
            .Include(reservation => reservation.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> GetByUserAsync(int userId)
    {
        return await _context.Reservations
            .Where(reservation => reservation.UserId == userId)
            .ToListAsync();
    }

    public async Task<Reservation?> GetReservationByIdAsync(int id)
    {
        return await _context.Reservations
            .Include(reservation => reservation.User)
            .FirstOrDefaultAsync(reservation => reservation.Id == id);
    }

    public async Task<bool> IsSlotAvailableAsync(DateTime start, DateTime end, int facilityId)
    {
        return !await _context.Reservations
            .AnyAsync(reservation =>
                reservation.FacilityId == facilityId &&
                ((start >= reservation.StartTime && start < reservation.EndTime) ||
                 (end > reservation.StartTime && end <= reservation.EndTime) ||
                 (start <= reservation.StartTime && end >= reservation.EndTime))
            );
    }

    public async Task<Reservation?> UpdateAsync(Reservation reservation)
    {
        _context.Reservations.Update(reservation);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Reservation {ReservationId} updated.", reservation.Id);

        return reservation;
    }

    public async Task<IEnumerable<Reservation>> GetByFacilityAndRangeAsync(int facilityId, DateTime? start = null, DateTime? end = null)
    {
        var query = _context.Reservations.AsQueryable();

        query = query.Where(reservation => reservation.FacilityId == facilityId);

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

    public async Task<IEnumerable<Reservation>> SearchAsync(int? userId = null, int? facilityId = null, DateTime? start = null, DateTime? end = null)
    {
        var query = _context.Reservations.AsQueryable();

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
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null)
        {
            _logger.LogWarning("Attempted to update status for non-existant reservation {ReservationId}", id);
            throw new InvalidOperationException("Reservation not found.");
        }

        reservation.Status = status;
        reservation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Reservation {ReservationId} status updated to {Status}", id, status);

        return reservation;
    }

    public async Task<bool> HasConflictAsync(int facilityId, DateTime start, DateTime end, int excludeReservationId)
    {
        var conflictExists = await _context.Reservations.AnyAsync(reservation =>
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