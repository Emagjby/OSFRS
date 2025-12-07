using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OSFRS.Backend.Data;
using OSFRS.Backend.Interfaces.Base;
using OSFRS.Backend.Interfaces.Logging;

namespace OSFRS.Backend.Repositories;

/// <summary>
/// Generic Entity Framework Core repository implementation providing
/// common CRUD operations and query support for any entity type.
/// </summary>
/// <typeparam name="TEntity">The entity type handled by the repository.</typeparam>
public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
{
    protected readonly OSFRSDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;
    protected readonly IAppLogger<BaseRepository<TEntity>> _logger;

    private readonly string _entityName;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseRepository{TEntity}"/> class.
    /// </summary>
    /// <param name="context">The EF Core database context.</param>
    /// <param name="logger">The application logger.</param>
    public BaseRepository(OSFRSDbContext context, IAppLogger<BaseRepository<TEntity>> logger)
    {
        _context = context;
        _dbSet = _context.Set<TEntity>();
        _logger = logger;
        _entityName = typeof(TEntity).Name;
    }

    /// <summary>
    /// Adds a new entity to the database context without saving changes.
    /// </summary>
    /// <param name="entity">The entity instance to add.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The added entity.</returns>
    public virtual async Task<TEntity?> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding new {Entity}", _entityName);

        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    /// <summary>
    /// Adds a collection of entities to the database context without saving changes.
    /// </summary>
    /// <param name="entities">The entities to add.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A read-only list of all added entities.</returns>
    public virtual async Task<IReadOnlyList<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding multiple {Entity} records", _entityName);

        await _dbSet.AddRangeAsync(entities, cancellationToken);
        return entities.ToList();
    }

    /// <summary>
    /// Checks whether an entity with the given identifier exists in the database.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>True if the entity exists; otherwise false.</returns>
    public virtual async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking existence of {Entity} with Id={Id}", _entityName, id);

        var entity = await _dbSet.FindAsync([id], cancellationToken);
        var exists = entity is not null;

        _logger.LogInformation("{Entity} with Id={Id} exists={Exists}", _entityName, id, exists);

        return exists;
    }

    /// <summary>
    /// Retrieves all entities matching the provided LINQ predicate.
    /// </summary>
    /// <param name="predicate">A filtering expression.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A read-only list of matching entities.</returns>
    public virtual async Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("{Entity}: Executing FindAsync with expression {Expression}", _entityName, predicate);

        return await _dbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves all entities of this type without enabling change tracking.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A read-only list of entities optimized for queries where updates are not required.
    /// </returns>
    public virtual async Task<IReadOnlyList<TEntity>> GetAllReadonlyAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all {Entity} records", _entityName);

        return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves all entities of this type with change tracking enabled.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A list of tracked entities suitable for update operations.</returns>
    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all {Entity} records", _entityName);

        return await _dbSet.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a single entity by its identifier.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The entity if found; otherwise null.</returns>
    public virtual async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting {Entity} with Id={Id}", _entityName, id);

        var entity = await _dbSet.FindAsync([id], cancellationToken);

        if (entity is null)
            _logger.LogWarning("{Entity} with Id={Id} not found", _entityName, id);

        return entity;
    }

    /// <summary>
    /// Creates a queryable source for advanced filtering and composition.
    /// </summary>
    /// <returns>An IQueryable representing the entity set.</returns>
    public IQueryable<TEntity> Query()
    {
        _logger.LogInformation("Creating Query() for {Entity}", _entityName);
        return _dbSet.AsNoTracking().AsQueryable();
    }

    /// <summary>
    /// Marks the given entity instance for deletion.
    /// </summary>
    /// <param name="entity">The entity instance to remove.</param>
    public void Remove(TEntity entity)
    {
        _logger.LogInformation("Removing {Entity}", _entityName);
        _dbSet.Remove(entity);
    }

    /// <summary>
    /// Saves all pending changes in the database context.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of affected rows.</returns>
    /// <exception cref="Exception">Thrown if the underlying database operation fails.</exception>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Saving changes for {Entity}", _entityName);

        try
        {
            int result = await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully saved changes for {Entity}. Affected rows: {Count}",
                _entityName,
                result
            );

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes for {Entity}", _entityName);
            throw;
        }
    }

    /// <summary>
    /// Marks the given entity instance as modified in the EF change tracker.
    /// </summary>
    /// <param name="entity">The entity instance to update.</param>
    public void Update(TEntity entity)
    {
        _logger.LogInformation("Updating {Entity}", _entityName);
        _dbSet.Update(entity);
    }
}