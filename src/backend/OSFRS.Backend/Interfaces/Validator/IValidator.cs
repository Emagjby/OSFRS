namespace OSFRS.Backend.Interfaces.Validator;

/// <summary>
/// Defines a validator responsible for enforcing business rules on a specific DTO type.
/// </summary>
/// <typeparam name="T">The type of the DTO being validated.</typeparam>
public interface IValidator<T>
{
    /// <summary>
    /// Validates the provided DTO instance and throws a domain-specific exception
    /// if any rule is violated.
    /// </summary>
    /// <param name="instance">The DTO instance to validate.</param>
    Task ValidateAsync(T instance);
}