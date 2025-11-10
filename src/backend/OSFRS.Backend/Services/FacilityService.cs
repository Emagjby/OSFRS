using OSFRS.Backend.DTOs;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Validators;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Services;

public class FacilityService : IFacilityService
{
    private readonly IFacilityRepository _repo;
    private readonly IAppLogger<FacilityService> _logger;

    public FacilityService(IFacilityRepository repo, IAppLogger<FacilityService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<Facility?> CreateFacilityAsync(CreateFacilityDto dto)
    {
        if (!FacilityValidator.ValidateCreate(dto, out var error))
            throw new ArgumentException(error);

        var facility = new Facility
        {
            Name = dto.Name,
            Type = dto.Type,
            Capacity = dto.Capacity,
            Status = dto.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _repo.AddAsync(facility);
        _logger.LogInformation("Created new facility '{Name}' successfully.");
        return result;
    }

    public async Task<bool> DeleteFacilityAsync(int id)
    {
        var result = await _repo.DeleteAsync(id);
        if (result)
            _logger.LogInformation("Deleted facility with ID {Id}.", id);
        else
            _logger.LogWarning("Failed to delete facility with ID {Id}", id);

        return result;
    }

    public async Task<IEnumerable<Facility>> GetAllFacilitiesAsync()
    {
        var facilities = await _repo.GetAllAsync();
        _logger.LogInformation("Retrieved {Count} facilities.", facilities.Count());
        return facilities;
    }

    public async Task<Facility?> GetFacilityByIdAsync(int id)
    {
        var facility = await _repo.GetByIdAsync(id);
        if (facility == null)
        {
            _logger.LogWarning("Facility with ID {Id} not found.", id);
            return null;
        }

        return facility;
    }

    public async Task<bool> IsFacilityAvailableAsync(int facilityId)
    {
        return await _repo.IsFacilityAvailableAsync(facilityId);
    }

    public async Task<bool> UpdateAvailabilityAsync(int facilityId, bool isAvailable)
    {
        var existing = await _repo.GetByIdAsync(facilityId);
        if (existing == null)
            throw new InvalidOperationException("Facility not found.");

        await _repo.UpdateAvailabilityAsync(facilityId, isAvailable);
        _logger.LogInformation("Facility {Id} marked as {Status}", facilityId, isAvailable ? "Available" : "Unavailable");
        return isAvailable;
    }

    public async Task<Facility?> UpdateFacilityAsync(int id, UpdateFacilityDto dto)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            throw new InvalidOperationException("Facility not found.");

        if (!FacilityValidator.ValidateUpdate(dto, existing, out var error))
            throw new ArgumentException(error);

        existing.Name = dto.Name ?? existing.Name;
        existing.Type = dto.Type ?? existing.Type;
        existing.Capacity = dto.Capacity ?? existing.Capacity;
        existing.Status = dto.Status ?? existing.Status;
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _repo.UpdateAsync(existing);
        _logger.LogInformation("Updated facility '{Name}' (ID: {Id})", updated.Name, updated.Id);

        return updated;
    }
}