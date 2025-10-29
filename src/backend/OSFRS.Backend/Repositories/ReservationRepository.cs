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

    public async Task AddAsync(Reservation reservation)
    {
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Reservation created for UserId {UserId}", reservation.UserId);
    }

    public async Task DeleteAsync(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if(reservation != null)
        {
            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Reservation {ReservationId} deleted.", id);
        }
        else
        {
            _logger.LogWarning("Attempted to delete non-existent reservation with ID {ReservationId}", id);
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

    public Task<bool> IsSlotAvailableAsync(DateTime start, DateTime end, int facilityId)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateAsync(Reservation reservation)
    {
        _context.Reservations.Update(reservation);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Reservation {ReservationId} updated.", reservation.Id);
    }
}