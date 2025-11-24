using OSFRS.Backend.DTOs.Maintenance;
using OSFRS.Backend.Exceptions;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Services;

/// <summary>
/// Provides operations for scheduling, updating, retrieving, and deleting
/// facility maintenance tasks. Also synchronizes facility availability
/// based on active maintenance windows.
/// </summary>
public class MaintenanceService : IMaintenanceService
{
    private readonly IMaintenanceRepository _repo;
    private readonly IFacilityRepository _facilityRepo;
    private readonly IAppLogger<MaintenanceService> _logger;

    private readonly IValidator<CreateMaintenanceRecordDto> _createValidator;
    private readonly IUpdateValidator<UpdateMaintenanceRecordDto, MaintenanceRecord> _updateValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="MaintenanceService"/> class.
    /// </summary>
    /// <param name="repo">Maintenance repository instance.</param>
    /// <param name="facilityRepo">Facility repository instance.</param>
    /// <param name="logger">Logging abstraction.</param>
    /// <param name="createValidator">Validator for creation of maintenance records.</param>
    /// <param name="updateValidator">Validator for updates to maintenance records.</param>
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

    /// <summary>
    /// Deletes a maintenance record by ID.
    /// </summary>
    /// <param name="id">Maintenance record ID.</param>
    /// <returns>
    /// True when the record existed and was removed; false if it did not exist.
    /// </returns>
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

    /// <summary>
    /// Retrieves all maintenance records associated with a facility.
    /// </summary>
    /// <param name="facilityId">Facility ID.</param>
    /// <returns>A collection of related maintenance records.</returns>
    /// <exception cref="NotFoundException">Thrown when the facility does not exist.</exception>
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

    /// <summary>
    /// Retrieves all upcoming future maintenance records.
    /// </summary>
    /// <returns>A collection of future-dated maintenance entries.</returns>
    public async Task<IEnumerable<MaintenanceRecord>> GetUpcomingMaintenanceAsync()
    {
        var records = await _repo.GetUpcomingAsync();

        _logger.LogInformation("Fetched {Count} upcoming maintenance records.", records.Count());
        return records;
    }

    /// <summary>
    /// Schedules a new maintenance task for a facility.
    /// </summary>
    /// <param name="dto">Creation request DTO.</param>
    /// <returns>The created <see cref="MaintenanceRecord"/> entity.</returns>
    /// <exception cref="NotFoundException">Thrown when the facility does not exist.</exception>
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

    /// <summary>
    /// Synchronizes maintenance and facility statuses based on current time.
    /// </summary>
    /// <remarks>
    /// This operation normalizes all maintenance records and updates related facility
    /// availability accordingly:
    /// <list type="bullet">
    /// <item>
    /// <description><c>Scheduled</c> — if the start time is in the future.</description>
    /// </item>
    /// <item>
    /// <description><c>InProgress</c> — if the current time falls between the record’s start and end.</description>
    /// </item>
    /// <item>
    /// <description><c>Completed</c> — if the end time has already passed.</description>
    /// </item>
    /// <item>
    /// <description><c>Cancelled</c> records are left unchanged.</description>
    /// </item>
    /// </list>
    /// Facilities linked to any <c>InProgress</c> maintenance are marked as
    /// <c>UnderMaintenance</c>. Facilities previously under maintenance with no
    /// active tasks are reverted to <c>Available</c>.
    /// </remarks>
    public async Task SyncStatusesAsync()
    {
        var now = DateTime.UtcNow;

        var records = await _repo.GetAllAsync();

        var facilitiesUnderMaintenance = new HashSet<int>();

        foreach (var record in records)
        {
            if (record.Status == "Cancelled")
                continue;

            if (record.EndTime < now)
            {
                record.Status = "Completed";
            }
            else if (record.StartTime <= now && record.EndTime > now)
            {
                record.Status = "InProgress";
                facilitiesUnderMaintenance.Add(record.FacilityId);
            }
            else if (record.StartTime > now)
            {
                record.Status = "Scheduled";
            }

            record.UpdatedAt = DateTime.UtcNow;
        }

        await _repo.SaveChangesAsync();

        // --------------------------------------
        // FACILITY STATUS SYNC
        // --------------------------------------
        var allFacilities = await _facilityRepo.GetAllAsync();

        foreach (var facility in allFacilities)
        {
            if (facility.Status == "Unavailable")
                continue;

            if (facilitiesUnderMaintenance.Contains(facility.Id))
            {
                facility.Status = "UnderMaintenance";
            }
            else
            {
                if (facility.Status == "UnderMaintenance")
                    facility.Status = "Available";
            }

            facility.UpdatedAt = DateTime.UtcNow;
        }

        await _facilityRepo.SaveChangesAsync();
    }

    /// <summary>
    /// Updates an existing maintenance record after validating the changes.
    /// </summary>
    /// <param name="id">Record ID.</param>
    /// <param name="dto">Update request DTO.</param>
    /// <returns>The updated record, or null if not found.</returns>
    /// <exception cref="NotFoundException">Thrown when the record does not exist.</exception>
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