using MonitoringPlatform.Application.Models;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace MonitoringPlatform.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        var statusCode = (int)HttpStatusCode.InternalServerError;
        var message = "Đã xảy ra lỗi trong hệ thống.";
        var errors = new List<ApiError>();

        // Log the exception
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        // Hide stack trace in production for security
        var stackTrace = _env.IsDevelopment() ? exception.StackTrace : null;

        var apiResponse = new ApiResponse<object>
        {
            Success = false,
            StatusCode = statusCode,
            Message = message,
            Data = null,
            Errors = errors,
            TraceId = traceId,
            Timestamp = DateTime.UtcNow
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(apiResponse, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
