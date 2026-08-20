using System.Text.Json;
using Pollynx.Application.DTOs.Common;

namespace Pollynx.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unhandled exception occurred.");

            await HandleExceptionAsync(
                context,
                exception);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var statusCode = exception switch
        {
            ArgumentException =>
                StatusCodes.Status400BadRequest,

            UnauthorizedAccessException =>
                StatusCodes.Status401Unauthorized,

            KeyNotFoundException =>
                StatusCodes.Status404NotFound,

            InvalidOperationException =>
                StatusCodes.Status409Conflict,

            _ =>
                StatusCodes.Status500InternalServerError
        };

        var code = exception switch
        {
            ArgumentException =>
                "VALIDATION_ERROR",

            UnauthorizedAccessException =>
                "UNAUTHORIZED",

            KeyNotFoundException =>
                "RESOURCE_NOT_FOUND",

            InvalidOperationException =>
                "BUSINESS_RULE_VIOLATION",

            _ =>
                "INTERNAL_SERVER_ERROR"
        };

        var message = exception switch
        {
            ArgumentException =>
                exception.Message,

            UnauthorizedAccessException =>
                exception.Message,

            KeyNotFoundException =>
                exception.Message,

            InvalidOperationException =>
                exception.Message,

            _ =>
                "An unexpected error occurred."
        };

        context.Response.StatusCode = statusCode;

        context.Response.ContentType = "application/json";

        var response = new ErrorResponseDto
        {
            Code = code,
            Message = message,
            TraceId = context.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(
                response,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy =
                        JsonNamingPolicy.CamelCase
                }));
    }
}