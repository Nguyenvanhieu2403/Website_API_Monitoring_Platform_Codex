using MonitoringPlatform.Domain.Entities;
using System.Security.Claims;

namespace MonitoringPlatform.Application.Interfaces;

public interface IJwtService
{
    Task<AuthTokens> GenerateTokensAsync(User user);
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    Task RevokeTokenAsync(Guid tokenId);
    Task RevokeAllTokensAsync(Guid userId);
}

public interface IPasswordHashingService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
    string HashToken(string token);
}

public record AuthTokens
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; init; }
    public DateTime RefreshTokenExpiresAt { get; init; }
}