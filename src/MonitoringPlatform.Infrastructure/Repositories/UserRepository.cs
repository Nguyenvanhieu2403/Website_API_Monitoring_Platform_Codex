using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Domain.Interfaces;
using MonitoringPlatform.Infrastructure.Data;

namespace MonitoringPlatform.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(ApplicationDbContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(Guid userId)
    {
        return await _context.Users
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<User?> GetByEmailAsync(string email, Guid? organizationId = null)
    {
        var query = _context.Users
            .Include(u => u.Organization)
            .AsQueryable();

        if (organizationId.HasValue)
        {
            query = query.Where(u => u.OrganizationId == organizationId.Value);
        }

        return await query.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> CreateAsync(User user)
    {
        user.UserId = Guid.NewGuid();
        user.CreatedAt = DateTime.UtcNow;

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} created", user.UserId);
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} updated", user.UserId);
    }

    public async Task DeleteAsync(Guid userId)
    {
        var user = await GetByIdAsync(userId);
        if (user != null)
        {
            user.Status = Domain.Enums.UserStatus.Inactive;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} deleted", userId);
        }
    }

    public async Task<IEnumerable<User>> GetByOrganizationAsync(Guid organizationId)
    {
        return await _context.Users
            .Include(u => u.Organization)
            .Where(u => u.OrganizationId == organizationId)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetByTeamAsync(Guid teamId)
    {
        return await _context.Users
            .Where(u => u.OrganizationId == teamId)
            .ToListAsync();
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(Guid tokenId)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.TokenId == tokenId);
    }

    public async Task UpdateRefreshTokenAsync(RefreshToken token)
    {
        _context.RefreshTokens.Update(token);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeAllRefreshTokensAsync(Guid userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(r => r.UserId == userId && r.RevokedAt == null)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
    {
        return await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.TokenHash == tokenHash);
    }

    public async Task<RefreshToken> CreateRefreshTokenAsync(RefreshToken token)
    {
        token.TokenId = Guid.NewGuid();
        token.CreatedAt = DateTime.UtcNow;

        await _context.RefreshTokens.AddAsync(token);
        await _context.SaveChangesAsync();

        return token;
    }
}