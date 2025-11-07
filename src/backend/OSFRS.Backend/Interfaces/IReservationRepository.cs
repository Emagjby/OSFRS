using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces;

public interface IReservationRepository
{
    Task<Reservation?> GetReservationByIdAsync(int id);
    Task<IEnumerable<Reservation>> GetByUserAsync(int userId);
    Task<IEnumerable<Reservation>> GetAllAsync();
    Task<Reservation?> AddAsync(Reservation reservation);
    Task<Reservation?> UpdateAsync(Reservation reservation);
    Task<Reservation?> UpdateStatusAsync(int id, string status);
    Task<bool> DeleteAsync(int id);
    Task<bool> IsSlotAvailableAsync(DateTime start, DateTime end, int facilityId);
    Task<IEnumerable<Reservation>> GetByFacilityAndRangeAsync(int facilityId, DateTime? start = null, DateTime? end = null);
    Task<IEnumerable<Reservation>> SearchAsync(int? userId = null, int? facilityId = null, DateTime? start = null, DateTime? end = null);
    Task<bool> HasConflictAsync(int facilityId, DateTime start, DateTime end, int excludeReservationId);
}