namespace OSFRS.Backend.Exceptions;

/// <summary>
/// Represents a 403 Forbidden domain-level error.
/// Thrown when a user attempts an action they are not permitted to perform,
/// even if they are authenticated. Automatically handled by the global
/// exception middleware.
/// </summary>
/// <param name="message">
/// A human-readable description explaining why the operation is forbidden.
/// </param>
public class ForbiddenException(string message) : Exception(message);