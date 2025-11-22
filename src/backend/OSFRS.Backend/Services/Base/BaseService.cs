using OSFRS.Backend.Interfaces.Base;

namespace OSFRS.Backend.Services;

/// <summary>
/// Provides the base functionality for read-only operations and DTO mapping
/// for any entity type.  
/// This class underpins all higher-level services such as CRUD services and
/// domain-specific services.
/// </summary>
/// <typeparam name="TEntity">Entity model type.</typeparam>
/// <typeparam name="TDto">DTO returned by the service.</typeparam>
public class BaseService<TEntity, TDto> : IBaseService<TEntity, TDto>
    where TEntity : class
    where TDto : class
{
    /// <summary>
    /// Underlying repository used for querying entities.
    /// </summary>
    protected readonly IBaseRepository<TEntity> _repo;

    /// <summary>
    /// Function used to map <typeparamref name="TEntity"/> to <typeparamref name="TDto"/>.
    /// </summary>
    protected readonly Func<TEntity, TDto> _mapToDto;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseService{TEntity, TDto}"/> class.
    /// </summary>
    /// <param name="repo">Repository instance for entity access.</param>
    /// <param name="mapToDto">DTO mapping function.</param>
    public BaseService(IBaseRepository<TEntity> repo, Func<TEntity, TDto> mapToDto)
    {
        _repo = repo;
        _mapToDto = mapToDto;
    }

    /// <summary>
    /// Retrieves all entities and maps them to DTOs.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A collection of mapped DTOs.</returns>
    public virtual async Task<IEnumerable<TDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repo.GetAllAsync(cancellationToken);
        return entities.Select(_mapToDto);
    }

    /// <summary>
    /// Retrieves a single entity by ID and maps it to its corresponding DTO,
    /// returning <c>null</c> if it does not exist.
    /// </summary>
    /// <param name="id">Entity ID.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The mapped DTO or <c>null</c> if the entity is not found.</returns>
    public virtual async Task<TDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repo.GetByIdAsync(id, cancellationToken);
        return entity is null ? null : _mapToDto(entity);
    }

    /// <summary>
    /// Maps an entity to its DTO representation using the service-level mapping function.
    /// </summary>
    /// <param name="entity">The entity instance to map.</param>
    /// <returns>The mapped DTO.</returns>
    public TDto MapToDto(TEntity entity) => _mapToDto(entity);
}