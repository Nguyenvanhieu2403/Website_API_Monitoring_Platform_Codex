using MediatR;
using MonitoringPlatform.Application.Interfaces;
using MonitoringPlatform.Application.Models;

namespace MonitoringPlatform.Application.Features.Authentication.Commands;

public record LoginCommand : IRequest<Result<LoginResponse>>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? RemoteIp { get; set; }
    public string? UserAgent { get; set; }
}

public record LoginResponse
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