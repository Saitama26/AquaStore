using System.Text.Json;
using AquaStore.Contracts.Common;
using Common.Application.Exceptions;

namespace AquaStore.Api.Middleware;

/// <summary>
/// Middleware для глобальной обработки исключений
/// </summary>
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, errorCode) = exception switch
        {
            NotFoundException notFound => (
                StatusCodes.Status404NotFound,
                notFound.Message,
                "NOT_FOUND"),

            ValidationException validation => (
                StatusCodes.Status400BadRequest,
                string.Join("; ", validation.Errors.Select(e => e.Message)),
                "VALIDATION_ERROR"),

            ForbiddenException forbidden => (
                StatusCodes.Status403Forbidden,
                forbidden.Message,
                "FORBIDDEN"),

            ConflictException conflict => (
                StatusCodes.Status409Conflict,
                conflict.Message,
                "CONFLICT"),

            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized access",
                "UNAUTHORIZED"),

            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred",
                "INTERNAL_ERROR")
        };

        context.Response.StatusCode = statusCode;

        var errors = new List<ApiError> { new(errorCode, message) };
        var response = ApiResponse<object>.Fail(message, errors);

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}

/// <summary>
/// Extension для регистрации middleware
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
