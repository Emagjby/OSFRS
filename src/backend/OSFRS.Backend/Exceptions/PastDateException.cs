namespace OSFRS.Backend.Exceptions;

/// <summary>
/// Represents an error thrown when a provided datetime value is earlier than the current UTC time.
/// </summary>
/// <remarks>
/// This exception is typically used by validators to enforce that entities such as reservations,
/// maintenance records, or scheduling operations cannot be created or updated with timestamps
/// that occur in the past. Automatically handled by the global exception middleware.
/// </remarks>
/// <param name="message">A descriptive explanation of the validation failure.</param>
public class PastDateException(string message) : Exception(message);