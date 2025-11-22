namespace OSFRS.Backend.Exceptions;

/// <summary>
/// Represents an error that occurs when a validation rule fails.
/// </summary>
/// <remarks>
/// Thrown by validators when input data does not meet required constraints,
/// such as invalid formats, missing fields, or logical inconsistencies.
/// Automatically mapped to HTTP 400 by the global exception middleware.
/// </remarks>
public class ValidationException(string message) : Exception(message);