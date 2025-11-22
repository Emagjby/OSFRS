using OSFRS.Backend.Exceptions;

namespace OSFRS.Backend.Validators.Base;

/// <summary>
/// Provides common validation helpers used across all validators.
/// Throws typed domain exceptions to ensure consistent error handling.
/// </summary>
public abstract class BaseValidator
{
    /// <summary>
    /// Ensures the given condition is true. Throws a <see cref="ValidationException"/> if false.
    /// </summary>
    /// <param name="condition">The condition to verify.</param>
    /// <param name="message">Error message included in the exception.</param>
    protected void Require(bool condition, string message)
    {
        if (!condition)
            throw new ValidationException(message);
    }

    /// <summary>
    /// Ensures a value is not null. Throws a <see cref="ValidationException"/> if null.
    /// </summary>
    /// <param name="value">Value to validate.</param>
    /// <param name="message">Error message included in the exception.</param>
    protected void RequireNotNull(object? value, string message)
    {
        if (value is null)
            throw new ValidationException(message);
    }

    /// <summary>
    /// Throws a <see cref="NotFoundException"/> when the entity is missing.
    /// Typically used for pre-checks before update or delete operations.
    /// </summary>
    /// <param name="entity">Entity to verify.</param>
    /// <param name="message">Error message included in the exception.</param>
    public void EnsureFound(object? entity, string message)
    {
        if (entity is null)
            throw new NotFoundException(message);
    }

    /// <summary>
    /// Ensures that no conflict occurred (e.g., resource already exists).
    /// Throws <see cref="ConflictException"/> if the provided condition is false.
    /// </summary>
    /// <param name="ok">Whether a conflict-free state was confirmed.</param>
    /// <param name="message">Error message included in the exception.</param>
    protected void EnsureNoConflict(bool ok, string message)
    {
        if (!ok)
            throw new ConflictException(message);
    }

    /// <summary>
    /// Immediately throws a <see cref="ForbiddenException"/>.
    /// Used when a user attempts an action without required permissions.
    /// </summary>
    /// <param name="message">Error message included in the exception.</param>
    protected void Forbidden(string message) => throw new ForbiddenException(message);

    /// <summary>
    /// Immediately throws an <see cref="UnauthorizedException"/>.
    /// </summary>
    /// <param name="message">Error message included in the exception.</param>
    protected void Unauthorized(string message) => throw new UnauthorizedException(message);

    /// <summary>
    /// Immediately throws a <see cref="ConflictException"/>.
    /// </summary>
    /// <param name="message">Error message included in the exception.</param>    
    protected void Conflict(string message) => throw new ConflictException(message);

    /// <summary>
    /// Validates that a date occurs in the future. Throws <see cref="PastDateException"/> otherwise.
    /// </summary>
    /// <param name="value">Date/time to validate.</param>
    /// <param name="message">Error message included in the exception.</param>
    protected void EnsureNotPast(DateTime value, string message)
    {
        if (value < DateTime.UtcNow)
            throw new PastDateException(message);
    }

    /// <summary>
    /// Ensures a time range is valid by checking that the start occurs before the end.
    /// Throws <see cref="ValidationException"/> if invalid.
    /// </summary>
    /// <param name="start">Start timestamp.</param>
    /// <param name="end">End timestamp.</param>
    /// <param name="message">Error message included in the exception.</param>
    protected void EnsureValidTimeRange(DateTime start, DateTime end, string message)
    {
        if (start >= end)
            throw new ValidationException(message);
    }

    /// <summary>
    /// Ensures an integer ID value is valid (&gt; 0). Throws <see cref="ValidationException"/> otherwise.
    /// </summary>
    /// <param name="id">Identifier to validate.</param>
    /// <param name="message">Error message included in the exception.</param>
    protected void EnsureValidId(int id, string message)
    {
        if (id <= 0)
            throw new ValidationException(message);
    }

    /// <summary>
    /// Ensures a <see cref="DateTime"/> value is in UTC format.
    /// Throws <see cref="ValidationException"/> if time is not <see cref="DateTimeKind.Utc"/>.
    /// </summary>
    /// <param name="value">Timestamp to validate.</param>
    /// <param name="message">Error message included in the exception.</param>
    protected void RequireUtc(DateTime value, string message)
    {
        if (value.Kind != DateTimeKind.Utc)
            throw new ValidationException(message);
    }
}