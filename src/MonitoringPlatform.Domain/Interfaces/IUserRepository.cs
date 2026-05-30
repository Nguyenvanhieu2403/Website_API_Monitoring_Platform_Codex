using MonitoringPlatform.Domain.Entities;

namespace MonitoringPlatform.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid userId);
    Task<User?> GetByEmailAsync(string email, Guid? organizationId = null);
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(Guid userId);
    Task<IEnumerable<User>> GetByOrganizationAsync(Guid organizationId);
    Task<RefreshToken?> GetRefreshTokenAsync(Guid tokenId);
    Task UpdateRefreshTokenAsync(RefreshToken token);
    Task RevokeAllRefreshTokensAsync(Guid userId);
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
    Task<RefreshToken> CreateRefreshTokenAsync(RefreshToken token);
}