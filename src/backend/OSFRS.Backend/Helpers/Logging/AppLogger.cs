using OSFRS.Backend.Interfaces.Logging;

namespace OSFRS.Backend.Helpers.Logging;

/// <summary>
/// Provides a thin abstraction over <see cref="ILogger{T}"/> to unify logging usage
/// across the application.
/// </summary>
/// <typeparam name="T">The category type associated with the logger.</typeparam>
public class AppLogger<T> : IAppLogger<T>
{
    private readonly ILogger<T> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppLogger{T}"/> class.
    /// </summary>
    /// <param name="logger">The underlying Microsoft logger instance.</param>
    public AppLogger(ILogger<T> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Logs an error with exception details.
    /// </summary>
    /// <param name="ex">The thrown exception.</param>
    /// <param name="message">The log message.</param>
    /// <param name="args">Structured logging arguments.</param>
    public void LogError(Exception ex, string message, params object[] args)
    {
        _logger.LogError(ex, message, args);
    }

    /// <summary>
    /// Logs an error with no exception details.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="args">Structured logging arguments.</param>
    public void LogError(string message, params object[] args)
    {
        _logger.LogError(message, args);
    }

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="args">Structured logging arguments.</param>
    public void LogInformation(string message, params object[] args)
    {
        _logger.LogInformation(message, args);
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="args">Structured logging arguments.</param>
    public void LogWarning(string message, params object[] args)
    {
        _logger.LogWarning(message, args);
    }
}