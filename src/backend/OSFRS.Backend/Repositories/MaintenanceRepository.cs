using Microsoft.EntityFrameworkCore;
using OSFRS.Backend.Data;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Repositories;

/// <summary>
/// Repository implementation for <see cref="MaintenanceRecord"/> entities,
/// extending the generic <see cref="BaseRepository{MaintenanceRecord}"/> with
/// maintenance-specific query operations.
/// </summary>
public class MaintenanceRepository : BaseRepository<MaintenanceRecord>, IMaintenanceRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MaintenanceRepository"/> class.
    /// </summary>
    /// <param name="context">The Entity Framework database context.</param>
    /// <param name="logger">The logger used for repository-level diagnostics.</param>
    public MaintenanceRepository(
        OSFRSDbContext context,
        IAppLogger<BaseRepository<MaintenanceRecord>> logger
    ) : base(context, logger)
    {
    }

    /// <summary>
    /// Retrieves all maintenance records associated with a specific facility.
    /// </summary>
    /// <param name="facilityId">The facility identifier.</param>
    /// <returns>
    /// A collection of <see cref="MaintenanceRecord"/> items ordered by descending start time.
    /// </returns>
    public async Task<IEnumerable<MaintenanceRecord>> GetByFacilityAsync(int facilityId)
    {
        _logger.LogInformation("Fetching maintenance records for Facility ID {FacilityId}", facilityId);

        return await _dbSet
            .Where(m => m.FacilityId == facilityId)
            .OrderByDescending(m => m.StartTime)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves all maintenance records scheduled to begin in the future.
    /// </summary>
    /// <returns>
    /// A collection of upcoming <see cref="MaintenanceRecord"/> items sorted by descending start time.
    /// </returns>
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