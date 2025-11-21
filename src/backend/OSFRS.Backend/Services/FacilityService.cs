using OSFRS.Backend.DTOs.Facilities;
using OSFRS.Backend.Exceptions;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Backend.Validators.Facilities;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Services;

public class FacilityService
    : BaseCrudService<Facility, CreateFacilityDto, UpdateFacilityDto, FacilityDto>,
      IFacilityService
{
    private new readonly IFacilityRepository _repo;
    private readonly IAppLogger<FacilityService> _logger;

    private readonly IValidator<CreateFacilityDto> _createValidator;
    private readonly IUpdateValidator<UpdateFacilityDto, Facility> _updateValidator;
    private readonly FacilityAvailabilityValidator _availabilityValidator;

    public FacilityService(
        IFacilityRepository repo,
        IAppLogger<FacilityService> logger,
        IValidator<CreateFacilityDto> createValidator,
        IUpdateValidator<UpdateFacilityDto, Facility> updateValidator,
        FacilityAvailabilityValidator availabilityValidator
    ) : base(
        repo,
        MapToDto,
        MapFromCreate,
        ApplyUpdate
    )
    {
        _repo = repo;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _availabilityValidator = availabilityValidator;
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
        if (dto.Name is not null) entity.Name = dto.Name;
        if (dto.Type is not null) entity.Type = dto.Type;
        if (dto.Capacity is not null) entity.Capacity = dto.Capacity.Value;
        if (dto.Status is not null) entity.Status = dto.Status;

        entity.UpdatedAt = DateTime.UtcNow;
    }

    public override async Task<FacilityDto> CreateAsync(CreateFacilityDto dto, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAsync(dto);

        _logger.LogInformation("Creating facility {Name}", dto.Name);

        return await base.CreateAsync(dto, cancellationToken);
    }

    public async Task<FacilityDto?> UpdateFacilityAsync(
        int id,
        UpdateFacilityDto dto,
        CancellationToken cancellationToken = default
    )
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing is null)
            throw new NotFoundException("Facility not found.");

        await _updateValidator.ValidateAsync(dto, existing);

        _logger.LogInformation("Updated facility {Id}", id);

        return await base.UpdateAsync(id, dto, cancellationToken);
    }

    public async Task<bool> UpdateAvailabilityAsync(int facilityId, bool isAvailable)
    {
        var existing = await _repo.GetByIdAsync(facilityId);
        if (existing is null)
            throw new NotFoundException("Facility not found.");

        await _availabilityValidator.ValidateAsync(existing, isAvailable);

        await _repo.UpdateAvailabilityAsync(facilityId, isAvailable);
        await _repo.SaveChangesAsync();

        _logger.LogInformation(
            "Facility {Id} marked as {Status}",
            facilityId,
            isAvailable ? "Available" : "Unavailable"
        );

        return isAvailable;
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

    public async Task<bool> IsFacilityAvailableAsync(int facilityId) => await _repo.IsFacilityAvailableAsync(facilityId);
}