using Microsoft.EntityFrameworkCore;
using OSFRS.Backend.Data;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Repositories;

public class MaintenanceRepository : IMaintenanceRepository
{
    private readonly OSFRSDbContext _context;
    private readonly IAppLogger<MaintenanceRepository> _logger;

    public MaintenanceRepository(OSFRSDbContext context, IAppLogger<MaintenanceRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<MaintenanceRecord> AddAsync(MaintenanceRecord maintenanceRecord)
    {
        _context.MaintenanceRecords.Add(maintenanceRecord);
        await _context.SaveChangesAsync();
        _logger.LogInformation(
            "Added maintenance record for facility ID {FacilityId} from {Start} to {End}.",
            maintenanceRecord.FacilityId,
            maintenanceRecord.StartTime,
            maintenanceRecord.EndTime
        );

        return maintenanceRecord;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var maintenanceRecord = await _context.MaintenanceRecords.FindAsync(id);
        if (maintenanceRecord == null)
        {
            _logger.LogWarning("Attempted to delete non-existent maintenance record ID {Id}.", id);
            return false;
        }

        _context.MaintenanceRecords.Remove(maintenanceRecord);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Deleted maintenance record ID {Id} for facility ID {FacilityId}.", id, maintenanceRecord.FacilityId);
        return true;
    }

    public async Task<IEnumerable<MaintenanceRecord>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all maintenance records...");
        var records = await _context.MaintenanceRecords
            .OrderByDescending(m => m.StartTime)
            .ToListAsync();
        _logger.LogInformation("Fetched {Count} maintenance records.", records.Count);
        return records;
    }

    public async Task<IEnumerable<MaintenanceRecord>> GetByFacilityAsync(int facilityId)
    {
        _logger.LogInformation("Fetching maintenance records for facility ID {FacilityId}...", facilityId);
        var records = await _context.MaintenanceRecords
            .Where(m => m.FacilityId == facilityId)
            .OrderByDescending(m => m.StartTime)
            .ToListAsync();
        _logger.LogInformation("Fetched {Count} maintenance records for facility ID {FacilityId}.", records.Count, facilityId);
        return records;
    }

    public async Task<MaintenanceRecord?> GetByIdAsync(int id)
    {
        var maintenanceRecord = await _context.MaintenanceRecords.FindAsync(id);
        if (maintenanceRecord == null)
        {
            _logger.LogWarning("Maintenance record ID {Id} not found.", id);
        }
        else
        {
            _logger.LogInformation("Fetched maintenance record ID {Id} for facility ID {FacilityId}.", id, maintenanceRecord.FacilityId);
        }
        return maintenanceRecord;
    }

    public async Task<IEnumerable<MaintenanceRecord>> GetUpcomingAsync()
    {
        var now = DateTime.UtcNow;
        _logger.LogInformation("Fetching upcoming maintenance records after {Now}...", now);
        var records = await _context.MaintenanceRecords
            .Where(m => m.StartTime >= now)
            .OrderByDescending(m => m.StartTime)
            .ToListAsync();
        _logger.LogInformation("Fetched {Count} upcoming maintenance records.", records.Count);
        return records;
    }

    public async Task<MaintenanceRecord> UpdateAsync(MaintenanceRecord maintenanceRecord)
    {
        var existing = await _context.MaintenanceRecords.FindAsync(maintenanceRecord.Id);
        if (existing == null)
        {
            _logger.LogWarning("Attempted to update non-existent maintenance record ID {Id}.", maintenanceRecord.Id);
            throw new InvalidOperationException("Maintenance Record not found.");
        }

        existing.Description = maintenanceRecord.Description;
        existing.StartTime = maintenanceRecord.StartTime;
        existing.EndTime = maintenanceRecord.EndTime;
        existing.Status = maintenanceRecord.Status;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated maintenance record ID {Id} for facility ID {FacilityId}.",
            existing.Id, existing.FacilityId);
        return existing;
    }
}