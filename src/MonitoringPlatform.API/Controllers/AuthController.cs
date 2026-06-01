using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringPlatform.Application.Features.Authentication.Commands;
using MonitoringPlatform.Application.Models;
using MonitoringPlatform.Application.Interfaces;
using MonitoringPlatform.Infrastructure.Security;
using Swashbuckle.AspNetCore.Annotations;

namespace MonitoringPlatform.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly IJwtService _jwtService;

    public AuthController(IMediator mediator, IJwtService jwtService)
    {
        _mediator = mediator;
        _jwtService = jwtService;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="command">Registration details</param>
    /// <returns>User registration response with tokens</returns>
    [HttpPost("register")]
    [SwaggerOperation(Summary = "Register a new user")]
    [SwaggerResponse(201, "User registered successfully", typeof(ApiResponse<RegisterResponse>))]
    [SwaggerResponse(400, "Invalid request", typeof(ApiResponse<object>))]
    public async Task<ActionResult<ApiResponse<RegisterResponse>>> Register([FromBody] RegisterCommand command)
    {
        command.RemoteIp = GetClientIp();
        command.UserAgent = GetUserAgent();

        var result = await _mediator.Send(command);
        return HandleResult(result, StatusCodes.Status201Created, "Người dùng đã được đăng ký thành công.");
    }

    /// <summary>
    /// Login user
    /// </summary>
    /// <param name="command">Login credentials</param>
    /// <returns>Login response with tokens</returns>
    [HttpPost("login")]
    [SwaggerOperation(Summary = "Login user")]
    [SwaggerResponse(200, "Login successful", typeof(ApiResponse<LoginResponse>))]
    [SwaggerResponse(401, "Invalid credentials", typeof(ApiResponse<object>))]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginCommand command)
    {
        command.RemoteIp = GetClientIp();
        command.UserAgent = GetUserAgent();

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return StatusCode(StatusCodes.Status401Unauthorized, new ApiResponse<LoginResponse>
            {
                Success = false,
                StatusCode = 401,
                Message = result.Error,
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }

        return Ok(new ApiResponse<LoginResponse>
        {
            Success = true,
            StatusCode = 200,
            Message = "Đăng nhập thành công.",
            Data = result.Value,
            TraceId = HttpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>New access and refresh tokens</returns>
    [HttpPost("refresh")]
    [SwaggerOperation(Summary = "Refresh access token")]
    [SwaggerResponse(200, "Token refreshed successfully", typeof(ApiResponse<AuthTokens>))]
    [SwaggerResponse(401, "Invalid or expired refresh token", typeof(ApiResponse<object>))]
    public async Task<ActionResult<ApiResponse<AuthTokens>>> Refresh([FromBody] RefreshTokenRequest request)
    {
        var tokenHash = new PasswordHashingService().HashToken(request.RefreshToken);
        var refreshToken = _jwtService.GetPrincipalFromExpiredToken(request.RefreshToken);

        if (refreshToken == null)
        {
            return StatusCode(StatusCodes.Status401Unauthorized, new ApiResponse<AuthTokens>
            {
                Success = false,
                StatusCode = 401,
                Message = "Token làm mới không hợp lệ.",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }

        var userId = Guid.Parse(refreshToken.FindFirst(c => c.Type == "sub")?.Value ?? string.Empty);
        var tokens = await _jwtService.GenerateTokensAsync(new Domain.Entities.User
        {
            UserId = userId
        });

        return Ok(new ApiResponse<AuthTokens>
        {
            Success = true,
            StatusCode = 200,
            Message = "Làm mới token thành công.",
            Data = tokens,
            TraceId = HttpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Logout user (revoke all tokens)
    /// </summary>
    /// <returns>Logout confirmation</returns>
    [Authorize]
    [HttpPost("logout")]
    [SwaggerOperation(Summary = "Logout user")]
    [SwaggerResponse(200, "Logout successful", typeof(ApiResponse<object>))]
    public async Task<IActionResult> Logout()
    {
        var userId = GetCurrentUserId();
        await _jwtService.RevokeAllTokensAsync(userId);
        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = 200,
            Message = "Đăng xuất thành công.",
            TraceId = HttpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    /// <returns>Current user profile</returns>
    [Authorize]
    [HttpGet("me")]
    [SwaggerOperation(Summary = "Get current user profile")]
    [SwaggerResponse(200, "User profile retrieved successfully", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = 200,
            Message = "Lấy thông tin người dùng thành công.",
            Data = new { userId },
            TraceId = HttpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        });
    }
}

public record RefreshTokenRequest(string RefreshToken);