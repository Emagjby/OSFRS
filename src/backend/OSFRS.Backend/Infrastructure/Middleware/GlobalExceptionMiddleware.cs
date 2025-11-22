using System.Net;
using System.Text.Json;
using OSFRS.Backend.Exceptions;
using OSFRS.Backend.Interfaces.Logging;

namespace OSFRS.Backend.Middleware;

/// <summary>
/// Global exception-handling middleware that intercepts unhandled exceptions
/// and converts them into structured HTTP responses.
/// </summary>
/// <remarks>
/// This middleware ensures:
/// <list type="bullet">
///   <item>Consistent error formatting across the entire API</item>
///   <item>Mapping of custom domain exceptions to appropriate HTTP status codes</item>
///   <item>Centralized logging of all unhandled exceptions</item>
/// </list>
/// It should be registered early in the pipeline so it can capture exceptions
/// thrown by controllers, services, validators, and repositories.
/// </remarks>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAppLogger<GlobalExceptionMiddleware> _logger;

    /// <summary>
    /// Creates a new instance of the global exception middleware.
    /// </summary>
    /// <param name="next">Delegate to invoke the next component in the pipeline.</param>
    /// <param name="logger">Logger used to record exception details.</param>
    public GlobalExceptionMiddleware(RequestDelegate next, IAppLogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Executes the middleware logic for the current HTTP context.
    /// </summary>
    /// <param name="context">The current HTTP request/response context.</param>
    /// <returns>A task representing the middleware execution.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Converts a thrown exception into a standardized JSON HTTP response.
    /// </summary>
    /// <param name="ctx">The current HTTP context.</param>
    /// <param name="ex">The exception that was thrown.</param>
    private async Task HandleExceptionAsync(HttpContext ctx, Exception ex)
    {
        var (status, message) = ex switch
        {
            ValidationException => (HttpStatusCode.BadRequest, ex.Message),
            NotFoundException => (HttpStatusCode.NotFound, ex.Message),
            ConflictException => (HttpStatusCode.Conflict, ex.Message),
            ForbiddenException => (HttpStatusCode.Forbidden, ex.Message),
            UnauthorizedException => (HttpStatusCode.Unauthorized, ex.Message),
            PastDateException => (HttpStatusCode.BadRequest, ex.Message),

            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        _logger.LogError(ex, "Exception caught by global handler: {Message}", ex.Message);

        ctx.Response.ContentType = "application/json";
        ctx.Response.StatusCode = (int)status;

        var json = JsonSerializer.Serialize(new
        {
            error = message,
            status = (int)status
        });

        await ctx.Response.WriteAsync(json);
    }
}