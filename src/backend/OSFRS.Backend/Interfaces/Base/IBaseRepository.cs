using System.Linq.Expressions;

namespace OSFRS.Backend.Interfaces.Base;

/// <summary>
/// Defines the base contract for repository operations used to interact with persistent entities.
/// </summary>
/// <typeparam name="TEntity">
/// The entity type managed by the repository.
/// </typeparam>
/// <remarks>
/// This interface provides core data-access operations, including CRUD primitives and
/// query helpers. All implementations are expected to interact with a database layer,
/// typically via Entity Framework Core.
/// </remarks>
public interface IBaseRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <param name="cancellationToken">A token used to observe cancellation requests.</param>
    /// <returns>
    /// The matching <typeparamref name="TEntity"/> instance,
    /// or <c>null</c> if no entity matches the given identifier.
    /// </returns>
    Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities of type <typeparamref name="TEntity"/> without enabling change tracking.
    /// </summary>
    /// <param name="cancellationToken">A token used to observe cancellation requests.</param>
    /// <returns>
    /// A read-only list of <typeparamref name="TEntity"/> instances intended for query operations.
    /// </returns>
    Task<IReadOnlyList<TEntity>> GetAllReadonlyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities of type <typeparamref name="TEntity"/> with change tracking enabled.
    /// </summary>
    /// <param name="cancellationToken">A token used to observe cancellation requests.</param>
    /// <returns>
    /// A list of tracked <typeparamref name="TEntity"/> instances suitable for update operations.
    /// </returns>
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);


    /// <summary>
    /// Retrieves entities that satisfy the provided predicate.
    /// </summary>
    /// <param name="predicate">A filter expression defining the query condition.</param>
    /// <param name="cancellationToken">A token used to observe cancellation requests.</param>
    /// <returns>
    /// A read-only list containing all <typeparamref name="TEntity"/> instances that match the predicate.
    /// </returns>
    Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Checks whether an entity with the specified identifier exists.
    /// </summary>
    /// <param name="id">The identifier to check.</param>
    /// <param name="cancellationToken">A token used to observe cancellation requests.</param>
    /// <returns>
    /// <c>true</c> if an entity with the specified identifier exists; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity to the data store.
    /// </summary>
    /// <param name="entity">The entity to persist.</param>
    /// <param name="cancellationToken">A token used to observe cancellation requests.</param>
    /// <returns>
    /// The newly created <typeparamref name="TEntity"/> instance, including any generated identifiers.
    /// </returns>
    Task<TEntity?> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a batch of new entities to the data store.
    /// </summary>
    /// <param name="entities">The entities to persist.</param>
    /// <param name="cancellationToken">A token used to observe cancellation requests.</param>
    /// <returns>
    /// A read-only list containing all created <typeparamref name="TEntity"/> instances.
    /// </returns>
    Task<IReadOnlyList<TEntity>> AddRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Marks an existing entity as modified.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    void Update(TEntity entity);

    /// <summary>
    /// Removes an entity from the data store.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    void Remove(TEntity entity);

    /// <summary>
    /// Persists all pending changes to the underlying database.
    /// </summary>
    /// <param name="cancellationToken">A token used to observe cancellation requests.</param>
    /// <returns>
    /// The number of state entries written to the database.
    /// </returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a queryable source for advanced LINQ queries.
    /// </summary>
    /// <returns>
    /// An <see cref="IQueryable{TEntity}"/> representing the entity set.
    /// </returns>
    IQueryable<TEntity> Query();
}