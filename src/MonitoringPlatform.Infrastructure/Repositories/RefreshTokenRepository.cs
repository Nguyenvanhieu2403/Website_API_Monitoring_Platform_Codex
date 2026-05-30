using Microsoft.EntityFrameworkCore;
using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Domain.Interfaces;
using MonitoringPlatform.Infrastructure.Data;

namespace MonitoringPlatform.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByIdAsync(Guid tokenId)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenId == tokenId);
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);
    }

    public async Task<RefreshToken> CreateAsync(RefreshToken token)
    {
        token.TokenId = Guid.NewGuid();
        token.CreatedAt = DateTime.UtcNow;

        await _context.RefreshTokens.AddAsync(token);
        await _context.SaveChangesAsync();

        return token;
    }

    public async Task UpdateAsync(RefreshToken token)
    {
        _context.RefreshTokens.Update(token);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeAllForUserAsync(Guid userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<int> DeleteExpiredAsync()
    {
        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();

        _context.RefreshTokens.RemoveRange(expiredTokens);
        return await _context.SaveChangesAsync();
    }
}

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByIdAsync(Guid tokenId);
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
    Task<RefreshToken> CreateAsync(RefreshToken token);
    Task UpdateAsync(RefreshToken token);
    Task RevokeAllForUserAsync(Guid userId);
    Task<int> DeleteExpiredAsync();
}