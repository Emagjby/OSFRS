namespace OSFRS.Backend.Interfaces.Validator;

/// <summary>
/// Defines validation logic for update operations where both the incoming update
/// payload and the existing persisted entity must be considered.
/// </summary>
/// <typeparam name="T">The DTO type containing updated values.</typeparam>
/// <typeparam name="TEntity">The entity type currently stored in the system.</typeparam>
public interface IUpdateValidator<T, TEntity>
{
    /// <summary>
    /// Validates an update request against business rules, considering both the
    /// provided update DTO and the existing entity instance.
    /// </summary>
    /// <param name="instance">The update DTO containing new values.</param>
    /// <param name="existing">The existing entity being updated.</param>
    Task ValidateAsync(T instance, TEntity existing);
}