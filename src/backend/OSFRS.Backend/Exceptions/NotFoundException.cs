namespace OSFRS.Backend.Exceptions;

/// <summary>
/// Thrown when a requested resource cannot be found in the system.
/// </summary>
/// <remarks>
/// This exception is used to indicate missing entities such as users,
/// facilities, reservations, or maintenance records. It should be returned
/// as HTTP 404 by the global exception middleware.
/// </remarks>
/// <param name="message">A descriptive message explaining what was not found.</param>
public class NotFoundException(string message) : Exception(message);