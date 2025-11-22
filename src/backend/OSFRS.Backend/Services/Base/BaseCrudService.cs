using OSFRS.Backend.DTOs.Facilities;
using OSFRS.Backend.Interfaces.Base;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Services;

/// <summary>
/// Provides a generic implementation of CRUD functionality for entities,
/// including creation, updating, deletion, and DTO mapping.  
/// Designed to be inherited by specific services such as <c>FacilityService</c>,
/// <c>MaintenanceService</c>, and others.
/// </summary>
/// <typeparam name="TEntity">Entity model type.</typeparam>
/// <typeparam name="TCreateDto">DTO used for creation.</typeparam>
/// <typeparam name="TUpdateDto">DTO used for updates.</typeparam>
/// <typeparam name="TDto">DTO returned to API consumers.</typeparam>
public class BaseCrudService<TEntity, TCreateDto, TUpdateDto, TDto>
    : BaseService<TEntity, TDto>, ICrudService<TCreateDto, TUpdateDto, TDto>
    where TEntity : class
    where TDto : class
{
    private readonly Func<TCreateDto, TEntity> _mapFromCreate;
    private readonly Action<TEntity, TUpdateDto> _mapFromUpdate;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseCrudService{TEntity, TCreateDto, TUpdateDto, TDto}"/> class.
    /// </summary>
    /// <param name="repo">Underlying repository used to persist and query entities.</param>
    /// <param name="mapToDto">Mapping function converting <typeparamref name="TEntity"/> to <typeparamref name="TDto"/>.</param>
    /// <param name="mapFromCreate">Factory function converting <typeparamref name="TCreateDto"/> to <typeparamref name="TEntity"/>.</param>
    /// <param name="mapFromUpdate">Action applying updates from <typeparamref name="TUpdateDto"/> onto an existing entity.</param>
    public BaseCrudService(
        IBaseRepository<TEntity> repo,
        Func<TEntity, TDto> mapToDto,
        Func<TCreateDto, TEntity> mapFromCreate,
        Action<TEntity, TUpdateDto> mapFromUpdate
    ) : base(repo, mapToDto)
    {
        _mapFromCreate = mapFromCreate;
        _mapFromUpdate = mapFromUpdate;
    }

    /// <summary>
    /// Creates a new entity from the provided DTO and persists it to the database.
    /// </summary>
    /// <param name="dto">Data required to construct a new entity.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The created entity mapped to <typeparamref name="TDto"/>.</returns>
    public virtual async Task<TDto> CreateAsync(TCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = _mapFromCreate(dto);
        await _repo.AddAsync(entity, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        return _mapToDto(entity);
    }

    /// <summary>
    /// Deletes an entity by ID if it exists.
    /// </summary>
    /// <param name="id">ID of the entity to remove.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>
    /// <c>true</c> if the entity was found and removed,  
    /// <c>false</c> if no entity matched the ID.
    /// </returns>
    public virtual async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repo.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return false;

        _repo.Remove(entity);
        await _repo.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// Updates an existing entity using the provided update DTO.
    /// </summary>
    /// <param name="id">Entity ID.</param>
    /// <param name="dto">Update DTO containing updated fields.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>
    /// The updated entity mapped to <typeparamref name="TDto"/>  
    /// or <c>null</c> if the entity does not exist.
    /// </returns>
    public virtual async Task<TDto?> UpdateAsync(int id, TUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repo.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return null;

        _mapFromUpdate(entity, dto);
        _repo.Update(entity);
        await _repo.SaveChangesAsync(cancellationToken);

        return _mapToDto(entity);
    }
}