using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces;

public interface IMaintenanceRepository : IBaseRepository<MaintenanceRecord>
{
    Task<IEnumerable<MaintenanceRecord>> GetByFacilityAsync(int facilityId);
    Task<IEnumerable<MaintenanceRecord>> GetUpcomingAsync();
}