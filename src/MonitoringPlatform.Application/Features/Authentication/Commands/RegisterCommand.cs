using MediatR;
using MonitoringPlatform.Application.Interfaces;

namespace MonitoringPlatform.Application.Features.Authentication.Commands;

public record RegisterCommand : IRequest<RegisterResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? OrganizationName { get; set; }
    public string? RemoteIp { get; set; }
    public string? UserAgent { get; set; }
}

public record RegisterResponse
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Guid OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public Domain.Enums.UserRole Role { get; set; }
    public AuthTokens Tokens { get; set; } = null!;
}

//public record AuthTokens
//{
//    public string AccessToken { get; set; } = string.Empty;
//    public string RefreshToken { get; set; } = string.Empty;
//    public DateTime AccessTokenExpiresAt { get; set; }
//    public DateTime RefreshTokenExpiresAt { get; set; }
//}