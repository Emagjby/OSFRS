using System.Net;
using System.Text.Json;
using OSFRS.Backend.Exceptions;
using OSFRS.Backend.Interfaces.Logging;

namespace OSFRS.Backend.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAppLogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, IAppLogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

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