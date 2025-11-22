namespace OSFRS.Backend.Interfaces.Logging;

/// <summary>
/// Defines an abstraction over the application logging system, providing
/// structured and type-scoped log operations.
/// </summary>
/// <typeparam name="T">
/// The category type used to scope log messages, typically the class
/// where the logger is injected.
/// </typeparam>
/// <remarks>
/// This interface wraps the underlying logging provider (e.g., Microsoft.Extensions.Logging)
/// to ensure consistent logging practices and make logging easily testable.
/// </remarks>
public interface IAppLogger<T>
{
    /// <summary>
    /// Writes a verbose informational message to the log.
    /// </summary>
    /// <param name="message">The log message template.</param>
    /// <param name="args">Optional structured log arguments.</param>
    void LogInformation(string message, params object[] args);

    /// <summary>
    /// Writes a warning indicating an unexpected or recoverable condition.
    /// </summary>
    /// <param name="message">The log message template.</param>
    /// <param name="args">Optional structured log arguments.</param>
    void LogWarning(string message, params object[] args);

    /// <summary>
    /// Writes an error message along with an associated exception.
    /// </summary>
    /// <param name="ex">The captured exception.</param>
    /// <param name="message">A descriptive message of the error scenario.</param>
    /// <param name="args">Optional structured log arguments.</param>
    void LogError(Exception ex, string message, params object[] args);
}