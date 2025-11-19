using OSFRS.Backend.Interfaces.Base;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces.Repository;

public interface IReservationRepository : IBaseRepository<Reservation>
{
    Task<IEnumerable<Reservation>> GetByUserAsync(int userId);
    Task<Reservation?> UpdateStatusAsync(int id, string status);
    Task<bool> IsSlotAvailableAsync(DateTime start, DateTime end, int facilityId);
    Task<IEnumerable<Reservation>> GetByFacilityAndRangeAsync(int facilityId, DateTime? start = null, DateTime? end = null);
    Task<IEnumerable<Reservation>> SearchAsync(int? userId = null, int? facilityId = null, DateTime? start = null, DateTime? end = null);
    Task<bool> HasConflictAsync(int facilityId, DateTime start, DateTime end, int excludeReservationId);
}