namespace OSFRS.Backend.Exceptions;

/// <summary>
/// Represents an error that occurs when a user attempts an action
/// without sufficient authentication.
/// </summary>
/// <remarks>
/// This exception is thrown when the caller is not logged in or when
/// authentication credentials are missing or invalid.  
/// Automatically handled by the global exception middleware and returned 
/// as HTTP 401 Unauthorized.
/// </remarks>
/// <param name="message">A description of the error.</param>
public class UnauthorizedException(string message) : Exception(message);