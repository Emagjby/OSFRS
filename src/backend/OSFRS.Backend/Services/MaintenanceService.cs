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

    public MaintenanceService(IMaintenanceRepository repo, IFacilityRepository facilityRepo, IAppLogger<MaintenanceService> logger)
    {
        _repo = repo;
        _facilityRepo = facilityRepo;
        _logger = logger;
    }

    public async Task<bool> DeleteMaintenanceAsync(int id)
    {
        var result = await _repo.DeleteAsync(id);
        if (result)
            _logger.LogInformation("Deleted maintenance record ID {Id}.", id);
        else
            _logger.LogWarning("Failed to delete maintenance record ID {Id}.", id);

        return result;
    }

    public async Task<IEnumerable<MaintenanceRecord>> GetMaintenanceByFacilityAsync(int facilityId)
    {
        var facility = await _facilityRepo.GetByIdAsync(facilityId);
        if (facility == null)
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
        if (facility == null)
        {
            _logger.LogWarning("Attempted to schedule maintenance for non-existent facility ID {FacilityId}.", dto.FacilityId);
            throw new InvalidOperationException("Facility not found.");
        }

        if (dto.EndTime <= dto.StartTime)
            throw new ArgumentException("End time must be after start time");

        var maintenanceRecord = new MaintenanceRecord
        {
            FacilityId = dto.FacilityId,
            Description = dto.Description,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Status = dto.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _repo.AddAsync(maintenanceRecord);
        _logger.LogInformation(
            "Scheduled maintenance for facility ID {FacilityId} from {Start} to {End}.",
            dto.FacilityId,
            dto.StartTime,
            dto.EndTime
        );
        return result;
    }

    public async Task SyncFacilityStatusesAsync()
    {
        var now = DateTime.UtcNow;
        _logger.LogInformation("Starting facility status sync based on maintenance records at {Now}.", now);

        var allRecords = await _repo.GetAllAsync();

        foreach (var maintenanceRecord in allRecords)
        {
            var facility = await _facilityRepo.GetByIdAsync(maintenanceRecord.FacilityId);
            if (facility == null)
            {
                _logger.LogWarning(
                    "Facility ID {FacilityId} referenced by maintenance record {RecordId} not found.",
                    maintenanceRecord.FacilityId,
                    maintenanceRecord.Id
                );
                continue;
            }

            if (now >= maintenanceRecord.StartTime && now <= maintenanceRecord.EndTime)
            {
                if (facility.Status != "UnderMaintenance")
                {
                    facility.Status = "UnderMaintenance";
                    facility.UpdatedAt = DateTime.UtcNow;
                    await _facilityRepo.UpdateAsync(facility);
                    _logger.LogInformation("Facility ID {Id} marked as 'UnderMaintenance'.", facility.Id);
                }
            }
            else if (now > maintenanceRecord.EndTime && facility.Status == "UnderMaintenance")
            {
                facility.Status = "Available";
                facility.UpdatedAt = DateTime.UtcNow;
                await _facilityRepo.UpdateAsync(facility);
                _logger.LogInformation("Facility ID {Id} marked as 'Available' after maintenance ended.", facility.Id);
            }
        }
        
        _logger.LogInformation("Facility status sync completed.");
    }

    public async Task<MaintenanceRecord?> UpdateMaintenanceAsync(int id, UpdateMaintenanceRecordDto dto)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
        {
            _logger.LogWarning("Attempted to update non-existent maintenance record ID {Id}.", id);
            throw new InvalidOperationException("Maintenance record not found.");
        }

        if (dto.EndTime <= dto.StartTime && dto.EndTime != default && dto.StartTime != default)
            throw new ArgumentException("End time must be after start time");


        existing.Description = dto.Description ?? existing.Description;

        if (dto.StartTime.HasValue)
            existing.StartTime = dto.StartTime ?? existing.StartTime;

        if (dto.EndTime.HasValue)
            existing.EndTime = dto.EndTime ?? existing.EndTime;
        
        existing.Status = dto.Status ?? existing.Status;
        existing.UpdatedAt = DateTime.UtcNow;


        var updated = await _repo.UpdateAsync(existing);
        _logger.LogInformation(
            "Updated maintenance record ID {Id} for facility ID {FacilityId}.",
            updated.Id,
            updated.FacilityId
        );

        return updated;
    }
}