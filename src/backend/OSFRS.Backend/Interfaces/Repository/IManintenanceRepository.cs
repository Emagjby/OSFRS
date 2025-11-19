using OSFRS.Backend.Interfaces.Base;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces.Repository;

public interface IMaintenanceRepository : IBaseRepository<MaintenanceRecord>
{
    Task<IEnumerable<MaintenanceRecord>> GetByFacilityAsync(int facilityId);
    Task<IEnumerable<MaintenanceRecord>> GetUpcomingAsync();
}