namespace OSFRS.Backend.Interfaces.Base;

/// <summary>
/// Defines the contract for CRUD-capable services operating on DTO-based models.
/// </summary>
/// <typeparam name="TCreateDto">
/// The DTO type used when creating new entities.
/// </typeparam>
/// <typeparam name="TUpdateDto">
/// The DTO type used when updating existing entities.
/// </typeparam>
/// <typeparam name="TDto">
/// The DTO type returned by read operations.
/// </typeparam>
/// <remarks>
/// This abstraction standardizes create, update, and delete service patterns across the application.
/// Read operations are inherited from <see cref="IBaseReadService{TDto}"/>.
/// </remarks>
public interface ICrudService<TCreateDto, TUpdateDto, TDto> : IBaseReadService<TDto>
{
    /// <summary>
    /// Creates a new entity based on the provided data-transfer object.
    /// </summary>
    /// <param name="dto">The DTO containing creation data.</param>
    /// <param name="cancellationToken">A token for cancelling the operation.</param>
    /// <returns>
    /// A <typeparamref name="TDto"/> representation of the newly created entity.
    /// </returns>
    Task<TDto> CreateAsync(TCreateDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity identified by its ID using the provided update DTO.
    /// </summary>
    /// <param name="id">The identifier of the entity to update.</param>
    /// <param name="dto">The DTO containing updated field values.</param>
    /// <param name="cancellationToken">A token for cancelling the operation.</param>
    /// <returns>
    /// The updated <typeparamref name="TDto"/> if found, otherwise <c>null</c>.
    /// </returns>
    Task<TDto?> UpdateAsync(int id, TUpdateDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity identified by its ID.
    /// </summary>
    /// <param name="id">The identifier of the entity to delete.</param>
    /// <param name="cancellationToken">A token for cancelling the operation.</param>
    /// <returns>
    /// <c>true</c> if the entity was deleted successfully, otherwise <c>false</c>.
    /// </returns>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}