namespace OSFRS.Backend.Interfaces.Base;

/// <summary>
/// Defines read-only operations for accessing DTO resources.
/// </summary>
/// <typeparam name="TDto">
/// The DTO type returned by the read operations.
/// </typeparam>
/// <remarks>
/// This interface provides the fundamental read operations used across most services.
/// It does not allow creation, update, or deletion - only retrieval.
/// </remarks>
public interface IBaseReadService<TDto>
{
    /// <summary>
    /// Retrieves a single resource by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the resource.</param>
    /// <param name="cancellationToken">A token used to observe cancellation requests.</param>
    /// <returns>
    /// The matching <typeparamref name="TDto"/> instance,
    /// or <c>null</c> if the resource does not exist.
    /// </returns>
    Task<TDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all resources of type <typeparamref name="TDto"/>.
    /// </summary>
    /// <param name="cancellationToken">A token used to observe cancellation requests.</param>
    /// <returns>
    /// A collection of all available <typeparamref name="TDto"/> records.
    /// </returns>
    Task<IEnumerable<TDto>> GetAllAsync(CancellationToken cancellationToken = default);
}