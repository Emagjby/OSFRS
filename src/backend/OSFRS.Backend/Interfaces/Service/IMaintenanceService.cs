using OSFRS.Backend.DTOs.Maintenance;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces.Service;

public interface IMaintenanceService
{
    Task<MaintenanceRecord> ScheduleMaintenanceAsync(CreateMaintenanceRecordDto dto);
    Task<MaintenanceRecord?> UpdateMaintenanceAsync(int id, UpdateMaintenanceRecordDto dto);
    Task<bool> DeleteMaintenanceAsync(int id);

    Task<IEnumerable<MaintenanceRecord>> GetMaintenanceByFacilityAsync(int facilityId);
    Task<IEnumerable<MaintenanceRecord>> GetUpcomingMaintenanceAsync();
    public Task SyncFacilityStatusesAsync();
}