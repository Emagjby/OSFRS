using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OSFRS.Backend.Data;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Base;
using OSFRS.Backend.Interfaces.Logging;

namespace OSFRS.Backend.Repositories;

public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
{
    protected readonly OSFRSDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;
    protected readonly IAppLogger<BaseRepository<TEntity>> _logger;

    private readonly string _entityName;

    public BaseRepository(OSFRSDbContext context, IAppLogger<BaseRepository<TEntity>> logger)
    {
        _context = context;
        _dbSet = _context.Set<TEntity>();
        _logger = logger;
        _entityName = typeof(TEntity).Name;
    }

    public virtual async Task<TEntity?> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding new {Entity}", _entityName);

        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual async Task<IReadOnlyList<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding multiple {Entity} records", _entityName);

        await _dbSet.AddRangeAsync(entities, cancellationToken);
        return entities.ToList();
    }

    public virtual async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking existence of {Entity} with Id={Id}", _entityName, id);

        var entity = await _dbSet.FindAsync([id], cancellationToken);
        var exists = entity is not null;

        _logger.LogInformation("{Entity} with Id={Id} exists={Exists}", _entityName, id, exists);

        return exists;
    }

    public virtual async Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("{Entity}: Executing FindAsync with expression {Expression}",
            _entityName, predicate);

        return await _dbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all {Entity} records", _entityName);

        return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public virtual async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting {Entity} with Id={Id}", _entityName, id);

        var entity = await _dbSet.FindAsync([id], cancellationToken);

        if (entity is null)
            _logger.LogWarning("{Entity} with Id={Id} not found", _entityName, id);

        return entity;
    }

    public IQueryable<TEntity> Query()
    {
        _logger.LogInformation("Creating Query() for {Entity}", _entityName);
        return _dbSet.AsNoTracking().AsQueryable();
    }

    public void Remove(TEntity entity)
    {
        _logger.LogInformation("Removing {Entity}", _entityName);

        _dbSet.Remove(entity);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Saving changes for {Entity}", _entityName);

        try
        {
            int result = await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully saved changes for {Entity}. Affected rows: {Count}",
                _entityName, result);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes for {Entity}", _entityName);
            throw;
        }
    }

    public void Update(TEntity entity)
    {
        _logger.LogInformation("Updating {Entity}", _entityName);

        _dbSet.Update(entity);
    }
}