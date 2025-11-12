using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces;

public interface IMaintenanceRepository
{
    Task<MaintenanceRecord?> GetByIdAsync(int id);
    Task<IEnumerable<MaintenanceRecord>> GetAllAsync();
    Task<IEnumerable<MaintenanceRecord>> GetByFacilityAsync(int facilityId);
    Task<IEnumerable<MaintenanceRecord>> GetUpcomingAsync();

    Task<MaintenanceRecord> AddAsync(MaintenanceRecord maintenanceRecord);
    Task<MaintenanceRecord> UpdateAsync(MaintenanceRecord maintenanceRecord);
    Task<bool> DeleteAsync(int id);
}