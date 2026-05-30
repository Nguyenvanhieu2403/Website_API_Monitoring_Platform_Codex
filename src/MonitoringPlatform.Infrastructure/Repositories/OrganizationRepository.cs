using Microsoft.EntityFrameworkCore;
using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Domain.Interfaces;
using MonitoringPlatform.Infrastructure.Data;

namespace MonitoringPlatform.Infrastructure.Repositories;

public class OrganizationRepository : IOrganizationRepository
{
    private readonly ApplicationDbContext _context;

    public OrganizationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Organization?> GetByIdAsync(Guid organizationId)
    {
        return await _context.Organizations
            .FirstOrDefaultAsync(o => o.OrganizationId == organizationId);
    }

    public async Task<Organization?> GetBySlugAsync(string slug)
    {
        return await _context.Organizations
            .FirstOrDefaultAsync(o => o.Slug == slug);
    }

    public async Task<Organization> CreateAsync(Organization organization)
    {
        organization.OrganizationId = Guid.NewGuid();
        organization.CreatedAt = DateTime.UtcNow;

        await _context.Organizations.AddAsync(organization);
        await _context.SaveChangesAsync();

        return organization;
    }

    public async Task UpdateAsync(Organization organization)
    {
        organization.UpdatedAt = DateTime.UtcNow;
        _context.Organizations.Update(organization);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid organizationId)
    {
        var organization = await GetByIdAsync(organizationId);
        if (organization != null)
        {
            _context.Organizations.Remove(organization);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Organization>> GetAllAsync()
    {
        return await _context.Organizations.ToListAsync();
    }
}