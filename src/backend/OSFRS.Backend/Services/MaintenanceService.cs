using OSFRS.Backend.DTOs;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Services;

public class MaintenanceService : IMaintenanceService
{
    private readonly IMaintenanceRepository _repo;
    private readonly IFacilityRepository _facilityRepo;
    private readonly IAppLogger<MaintenanceService> _logger;

    public MaintenanceService(
        IMaintenanceRepository repo,
        IFacilityRepository facilityRepo,
        IAppLogger<MaintenanceService> logger
    )
    {
        _repo = repo;
        _facilityRepo = facilityRepo;
        _logger = logger;
    }

    public async Task<bool> DeleteMaintenanceAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null)
        {
            _logger.LogWarning("Attempted to delete non-existent maintenance record ID {Id}", id);
            return false;
        }

        _repo.Remove(entity);
        await _repo.SaveChangesAsync();

        _logger.LogInformation("Deleted maintenance record ID {Id}.", id);

        return true;
    }

    public async Task<IEnumerable<MaintenanceRecord>> GetMaintenanceByFacilityAsync(int facilityId)
    {
        var facility = await _facilityRepo.GetByIdAsync(facilityId);
        if (facility is null)
        {
            _logger.LogWarning("Attempted to fetch maintenance for non-existent facility ID {FacilityId}.", facilityId);
            throw new InvalidOperationException("Facility not found.");
        }

        var records = await _repo.GetByFacilityAsync(facilityId);
        _logger.LogInformation(
            "Fetched {Count} maintenance records for facility ID {FacilityId}.",
            records.Count(),
            facilityId
        );

        return records;
    }

    public async Task<IEnumerable<MaintenanceRecord>> GetUpcomingMaintenanceAsync()
    {
        var records = await _repo.GetUpcomingAsync();
        _logger.LogInformation("Fetched {Count} upcoming maintenance records.", records.Count());
        return records;
    }

    public async Task<MaintenanceRecord> ScheduleMaintenanceAsync(CreateMaintenanceRecordDto dto)
    {
        var facility = await _facilityRepo.GetByIdAsync(dto.FacilityId);
        if (facility is null)
        {
            _logger.LogWarning("Attempted to schedule maintenance for non-existent facility ID {FacilityId}.", dto.FacilityId);
            throw new InvalidOperationException("Facility not found.");
        }

        if (dto.EndTime <= dto.StartTime)
            throw new ArgumentException("End time must be after start time");

        var entity = new MaintenanceRecord
        {
            FacilityId = dto.FacilityId,
            Description = dto.Description,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Status = dto.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();

        _logger.LogInformation(
            "Scheduled maintenance for facility ID {FacilityId} from {Start} to {End}.",
            dto.FacilityId,
            dto.StartTime,
            dto.EndTime
        );

        return entity;
    }

    public async Task SyncFacilityStatusesAsync()
    {
        var now = DateTime.UtcNow;
        _logger.LogInformation("Starting facility status sync based on maintenance records at {Now}.", now);

        var allMaintenance = await _repo.GetAllAsync();

        foreach (var m in allMaintenance)
        {
            var facility = await _facilityRepo.GetByIdAsync(m.FacilityId);
            if (facility is null)
            {
                _logger.LogWarning(
                    "Facility ID {FacilityId} referenced by maintenance record {RecordId} not found.",
                    m.FacilityId,
                    m.Id
                );
                continue;
            }

            var active = now >= m.StartTime && now <= m.EndTime;

            if (active && facility.Status != "UnderMaintenance")
            {
                facility.Status = "UnderMaintenance";
                facility.UpdatedAt = DateTime.UtcNow;

                _facilityRepo.Update(facility);
                await _facilityRepo.SaveChangesAsync();

                _logger.LogInformation("Facility ID {Id} marked as 'UnderMaintenance'.", facility.Id);
            }
            else if (!active && facility.Status == "UnderMaintenance")
            {
                facility.Status = "Available";
                facility.UpdatedAt = DateTime.UtcNow;

                _facilityRepo.Update(facility);
                await _facilityRepo.SaveChangesAsync();

                _logger.LogInformation("Facility ID {Id} marked as 'Available'.", facility.Id);
            }
        }
        
        _logger.LogInformation("Facility status sync completed.");
    }

    public async Task<MaintenanceRecord?> UpdateMaintenanceAsync(int id, UpdateMaintenanceRecordDto dto)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null)
        {
            _logger.LogWarning("Attempted to update non-existent maintenance record ID {Id}.", id);
            throw new InvalidOperationException("Maintenance record not found.");
        }

        if (dto.EndTime <= dto.StartTime && dto.EndTime != default)
            throw new ArgumentException("End time must be after start time");


        entity.Description = dto.Description ?? entity.Description;

        if (dto.StartTime.HasValue)
            entity.StartTime = dto.StartTime.Value;

        if (dto.EndTime.HasValue)
            entity.EndTime = dto.EndTime.Value;
        
        entity.Status = dto.Status ?? entity.Status;
        entity.UpdatedAt = DateTime.UtcNow;

        _repo.Update(entity);
        await _repo.SaveChangesAsync();

        _logger.LogInformation(
            "Updated maintenance record ID {Id} for facility ID {FacilityId}.",
            entity.Id,
            entity.FacilityId
        );

        return entity;
    }
}