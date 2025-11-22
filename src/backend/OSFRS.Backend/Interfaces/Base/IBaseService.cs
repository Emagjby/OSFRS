namespace OSFRS.Backend.Interfaces.Base;

/// <summary>
/// Defines the base contract for service-layer types that expose read operations
/// and entity-to-DTO mapping functionality.
/// </summary>
/// <typeparam name="TEntity">
/// The underlying domain entity type handled by the service.
/// </typeparam>
/// <typeparam name="TDto">
/// The data-transfer object type used to expose <typeparamref name="TEntity"/> externally.
/// </typeparam>
/// <remarks>
/// This abstraction ensures that all services built on top of <typeparamref name="TEntity"/>
/// provide a consistent mapping method and implement standard read operations through
/// <see cref="IBaseReadService{TDto}"/>.
/// </remarks>
public interface IBaseService<TEntity, TDto> : IBaseReadService<TDto>
{
    /// <summary>
    /// Maps a domain entity to its corresponding DTO representation.
    /// </summary>
    /// <param name="entity">The domain entity to map.</param>
    /// <returns>
    /// A <typeparamref name="TDto"/> instance representing the mapped entity.
    /// </returns>
    TDto MapToDto(TEntity entity);
}