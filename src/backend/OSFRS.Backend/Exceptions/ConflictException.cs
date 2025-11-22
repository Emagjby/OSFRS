namespace OSFRS.Backend.Exceptions;

/// <summary>
/// Represents an application-level conflict error.
/// </summary>
/// <remarks>
/// This exception is thrown when an operation cannot proceed because it would
/// violate a business rule or cause an inconsistent state.  
/// Common scenarios include attempting to create a resource that already exists,
/// updating an entity in a way that clashes with constraints, or performing an
/// action during a conflicting time window. Automatically handled by the
/// global exception middleware.
/// </remarks>
/// <param name="message">A descriptive message explaining the conflict.</param>
public class ConflictException(string message) : Exception(message);