using Microsoft.EntityFrameworkCore;
using OSFRS.Backend.Data;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Repositories;

public class MaintenanceRepository : BaseRepository<MaintenanceRecord>, IMaintenanceRepository
{
    public MaintenanceRepository(
        OSFRSDbContext context,
        IAppLogger<BaseRepository<MaintenanceRecord>> logger
    ) : base(context, logger)
    {
    }

    public async Task<IEnumerable<MaintenanceRecord>> GetByFacilityAsync(int facilityId)
    {
        _logger.LogInformation("Fetching maintenance records for Facility ID {FacilityId}", facilityId);

        return await _dbSet
            .Where(m => m.FacilityId == facilityId)
            .OrderByDescending(m => m.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<MaintenanceRecord>> GetUpcomingAsync()
    {
        var now = DateTime.UtcNow;
        _logger.LogInformation("Fetching upcoming maintenance records after {Now}", now);

        return await _dbSet
            .Where(m => m.StartTime >= now)
            .OrderByDescending(m => m.StartTime)
            .ToListAsync();
    }
}