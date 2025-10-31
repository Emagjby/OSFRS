using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces;

public interface IReservationRepository
{
    Task<Reservation?> GetReservationByIdAsync(int id);
    Task<IEnumerable<Reservation>> GetByUserAsync(int userId);
    Task<IEnumerable<Reservation>> GetAllAsync();
    Task AddAsync(Reservation reservation);
    Task UpdateAsync(Reservation reservation);
    Task DeleteAsync(int id);
    Task<bool> IsSlotAvailableAsync(DateTime start, DateTime end, int facilityId);
    Task<IEnumerable<Reservation>> GetByFacilityAndRangeAsync(int facilityId, DateTime? start = null, DateTime? end = null);
    Task<IEnumerable<Reservation>> SearchAsync(int? userId = null, int? facilityId = null, DateTime? start = null, DateTime? end = null);
}