using OSFRS.Backend.DTOs.Facilities;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.Backend.Validators;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Services;

public class FacilityService
    : BaseCrudService<Facility, CreateFacilityDto, UpdateFacilityDto, FacilityDto>,
      IFacilityService
{
    private new readonly IFacilityRepository _repo;
    private readonly IAppLogger<FacilityService> _logger;

    public FacilityService(
        IFacilityRepository repo,
        IAppLogger<FacilityService> logger
    ) : base(
        repo,
        MapToDto,
        MapFromCreate,
        ApplyUpdate
    )
    {
        _repo = repo;
        _logger = logger;
    }

    private new static FacilityDto MapToDto(Facility f) =>
        new FacilityDto
        {
            Id = f.Id,
            Name = f.Name,
            Type = f.Type,
            Capacity = f.Capacity,
            Status = f.Status,
        };

    private static Facility MapFromCreate(CreateFacilityDto dto) =>
        new Facility
        {
            Name = dto.Name,
            Type = dto.Type,
            Capacity = dto.Capacity,
            Status = dto.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    private static void ApplyUpdate(Facility entity, UpdateFacilityDto dto)
    {
        entity.Name = dto.Name ?? entity.Name;
        entity.Type = dto.Type ?? entity.Type;
        entity.Capacity = dto.Capacity ?? entity.Capacity;
        entity.Status = dto.Status ?? entity.Status;
        entity.UpdatedAt = DateTime.UtcNow;
    }

    public override async Task<FacilityDto> CreateAsync(CreateFacilityDto dto, CancellationToken cancellationToken = default)
    {
        if (!FacilityValidator.ValidateCreate(dto, out var error))
            throw new ArgumentException(error);

        _logger.LogInformation("Creating facility {Name}", dto.Name);

        return await base.CreateAsync(dto, cancellationToken);
    }

    public override async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var result = await base.DeleteAsync(id, cancellationToken);

        if (result)
            _logger.LogInformation("Deleted facility with ID {Id}.", id);
        else
            _logger.LogWarning("Failed to delete facility with ID {Id}", id);

        return result;
    }

    public override async Task<IEnumerable<FacilityDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var facilities = await base.GetAllAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} facilities.", facilities.Count());

        return facilities;
    }

    public override async Task<FacilityDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var facility = await base.GetByIdAsync(id, cancellationToken);

        if (facility is null)
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
        if (existing is null)
            throw new InvalidOperationException("Facility not found.");

        await _repo.UpdateAvailabilityAsync(facilityId, isAvailable);
        await _repo.SaveChangesAsync();

        _logger.LogInformation(
            "Facility {Id} marked as {Status}",
            facilityId,
            isAvailable ? "Available" : "Unavailable"
        );

        return isAvailable;
    }

    public async Task<FacilityDto?> UpdateFacilityAsync(
        int id,
        UpdateFacilityDto dto,
        CancellationToken cancellationToken = default
    )
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing is null)
            throw new InvalidOperationException("Facility not found.");

        if (!FacilityValidator.ValidateUpdate(dto, existing, out var error))
            throw new ArgumentException(error);

        _logger.LogInformation("Updated facility {Id}", id);

        return await base.UpdateAsync(id, dto, cancellationToken);
    }
}