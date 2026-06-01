using Microsoft.AspNetCore.Mvc;
using MonitoringPlatform.Application.Models;

namespace MonitoringPlatform.API.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected Guid GetOrganizationId()
    {
        var orgIdClaim = User.FindFirst("organization_id")?.Value;
        if (string.IsNullOrEmpty(orgIdClaim) || !Guid.TryParse(orgIdClaim, out var organizationId))
        {
            throw new UnauthorizedAccessException("Không tìm thấy ID tổ chức hoặc ID không hợp lệ.");
        }
        return organizationId;
    }

    protected Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Không tìm thấy ID người dùng hoặc ID không hợp lệ.");
        }
        return userId;
    }

    protected void ValidateOrganizationId(Guid organizationId)
    {
        if (GetOrganizationId() != organizationId)
        {
            throw new UnauthorizedAccessException("Không có quyền truy cập tổ chức này.");
        }
    }

    protected ActionResult<ApiResponse<T>> HandleResult<T>(Result<T> result, int successStatusCode, string successMessage)
    {
        if (!result.IsSuccess)
        {
            var statusCode = DetermineStatusCode(result.Error);
            return StatusCode(statusCode, new ApiResponse<T>
            {
                Success = false,
                StatusCode = statusCode,
                Message = result.Error,
                Errors = result.Errors,
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }

        return StatusCode(successStatusCode, new ApiResponse<T>
        {
            Success = true,
            StatusCode = successStatusCode,
            Message = successMessage,
            Data = result.Value,
            TraceId = HttpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        });
    }

    protected IActionResult HandleResult(Result result, int successStatusCode, string successMessage)
    {
        if (!result.IsSuccess)
        {
            var statusCode = DetermineStatusCode(result.Error);
            return StatusCode(statusCode, new ApiResponse<object>
            {
                Success = false,
                StatusCode = statusCode,
                Message = result.Error,
                Errors = result.Errors,
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }

        return StatusCode(successStatusCode, new ApiResponse<object>
        {
            Success = true,
            StatusCode = successStatusCode,
            Message = successMessage,
            TraceId = HttpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        });
    }

    private static int DetermineStatusCode(string errorMessage)
    {
        if (string.IsNullOrEmpty(errorMessage))
            return 400;

        var lowerError = errorMessage.ToLowerInvariant();

        if (lowerError.Contains("không tìm thấy") || lowerError.Contains("not found"))
            return 404;

        if (lowerError.Contains("không có quyền") || lowerError.Contains("unauthorized") ||
            lowerError.Contains("access denied") || lowerError.Contains("truy cập"))
            return 401;

        if (lowerError.Contains("đã tồn tại") || lowerError.Contains("already exists") ||
            lowerError.Contains("không hợp lệ") || lowerError.Contains("invalid"))
            return 400;

        return 400; // Default to BadRequest
    }

    protected string GetClientIp()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    protected string GetUserAgent()
    {
        return HttpContext.Request.Headers["User-Agent"].ToString();
    }
}
