using OSFRS.Backend.DTOs.Maintenance;
using OSFRS.Backend.Exceptions;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Services;

public class MaintenanceService : IMaintenanceService
{
    private readonly IMaintenanceRepository _repo;
    private readonly IFacilityRepository _facilityRepo;
    private readonly IAppLogger<MaintenanceService> _logger;

    private readonly IValidator<CreateMaintenanceRecordDto> _createValidator;
    private readonly IUpdateValidator<UpdateMaintenanceRecordDto, MaintenanceRecord> _updateValidator;

    public MaintenanceService(
        IMaintenanceRepository repo,
        IFacilityRepository facilityRepo,
        IAppLogger<MaintenanceService> logger,
        IValidator<CreateMaintenanceRecordDto> createValidator,
        IUpdateValidator<UpdateMaintenanceRecordDto, MaintenanceRecord> updateValidator
    )
    {
        _repo = repo;
        _facilityRepo = facilityRepo;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
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
        if (await _facilityRepo.GetByIdAsync(facilityId) is null)
            throw new NotFoundException("Facility not found.");

        var records = await _repo.GetByFacilityAsync(facilityId);

        _logger.LogInformation(
            "Fetched {Count} maintenance records for facility {FacilityId}",
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
        await _createValidator.ValidateAsync(dto);

        if (await _facilityRepo.GetByIdAsync(dto.FacilityId) is null)
            throw new NotFoundException("Facility not found.");

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
        var existing = await _repo.GetByIdAsync(id)
                        ?? throw new NotFoundException("Maintenance record not found.");

        await _updateValidator.ValidateAsync(dto, existing);

        existing.Description = dto.Description ?? existing.Description;
        existing.StartTime = dto.StartTime ?? existing.StartTime;
        existing.EndTime = dto.EndTime ?? existing.EndTime;
        existing.Status = dto.Status ?? existing.Status;
        existing.UpdatedAt = DateTime.UtcNow;

        _repo.Update(existing);
        await _repo.SaveChangesAsync();

        _logger.LogInformation(
            "Updated maintenance record {Id} for facility {FacilityId}",
            existing.Id, existing.FacilityId
        );

        return existing;
    }
}