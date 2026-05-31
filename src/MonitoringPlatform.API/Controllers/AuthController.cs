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
public class AuthController : ControllerBase
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

        if (!result.IsSuccess)
        {
            return BadRequest(new ApiResponse<RegisterResponse>
            {
                Success = false,
                StatusCode = 400,
                Message = result.Error,
                Errors = result.Errors
            });
        }

        return CreatedAtAction(nameof(Register), new ApiResponse<RegisterResponse>
        {
            Success = true,
            StatusCode = 201,
            Message = "Người dùng đã được đăng ký thành công.",
            Data = result.Value
        });
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
            return Unauthorized(new ApiResponse<LoginResponse>
            {
                Success = false,
                StatusCode = 401,
                Message = result.Error
            });
        }

        return Ok(new ApiResponse<LoginResponse>
        {
            Success = true,
            StatusCode = 200,
            Message = "Đăng nhập thành công.",
            Data = result.Value
        });
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>New access and refresh tokens</returns>
    [HttpPost("refresh")]
    [SwaggerOperation(Summary = "Refresh access token")]
    [SwaggerResponse(200, "Token refreshed successfully", typeof(AuthTokens))]
    [SwaggerResponse(401, "Invalid or expired refresh token")]
    public async Task<ActionResult<AuthTokens>> Refresh([FromBody] RefreshTokenRequest request)
    {
        var tokenHash = new PasswordHashingService().HashToken(request.RefreshToken);
        var refreshToken = _jwtService.GetPrincipalFromExpiredToken(request.RefreshToken);

        if (refreshToken == null)
        {
            return Unauthorized(new { message = "Invalid refresh token" });
        }

        var userId = Guid.Parse(refreshToken.FindFirst(c => c.Type == "sub")?.Value ?? string.Empty);
        var user = await _jwtService.GenerateTokensAsync(new Domain.Entities.User
        {
            UserId = userId
        });

        return Ok(user);
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
            Message = "Đăng xuất thành công."
        });
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    /// <returns>Current user profile</returns>
    [Authorize]
    [HttpGet("me")]
    [SwaggerOperation(Summary = "Get current user profile")]
    [SwaggerResponse(200, "User profile retrieved successfully")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        // Implementation would fetch user from repository
        return Ok(new { userId = userId });
    }

    private string GetClientIp()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private string GetUserAgent()
    {
        return HttpContext.Request.Headers["User-Agent"].ToString();
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(c => c.Type == "sub");
        return Guid.Parse(userIdClaim?.Value ?? throw new UnauthorizedAccessException());
    }
}

public record RefreshTokenRequest(string RefreshToken);