using OSFRS.Backend.Exceptions;

namespace OSFRS.Backend.Validators.Base;

public abstract class BaseValidator
{
    protected void Require(bool condition, string message)
    {
        if (!condition)
            throw new ValidationException(message);
    }

    protected void RequireNotNull(object? value, string message)
    {
        if (value is null)
            throw new ValidationException(message);
    }

    public void EnsureFound(object? entity, string message)
    {
        if (entity is null)
            throw new NotFoundException(message);
    }

    protected void EnsureNoConflict(bool ok, string message)
    {
        if (!ok)
            throw new ConflictException(message);
    }

    protected void Forbidden(string message) => throw new ForbiddenException(message);

    protected void Unauthorized(string message) => throw new UnauthorizedException(message);

    protected void Conflict(string message) => throw new ConflictException(message);

    protected void EnsureNotPast(DateTime value, string message)
    {
        if (value < DateTime.UtcNow)
            throw new PastDateException(message);
    }

    protected void EnsureValidTimeRange(DateTime start, DateTime end, string message)
    {
        if (start >= end)
            throw new ValidationException(message);
    }

    protected void EnsureValidId(int id, string message)
    {
        if (id <= 0)
            throw new ValidationException(message);
    }

    protected void RequireUtc(DateTime value, string message)
    {
        if (value.Kind != DateTimeKind.Utc)
            throw new ValidationException(message);
    }
}