using System.Linq.Expressions;

namespace OSFRS.Backend.Interfaces.Base;

public interface IBaseRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    Task<TEntity?> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    void Update(TEntity entity);

    void Remove(TEntity entity);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    IQueryable<TEntity> Query();
}