using Microsoft.EntityFrameworkCore;
using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Domain.Interfaces;
using MonitoringPlatform.Infrastructure.Data;

namespace MonitoringPlatform.Infrastructure.Repositories;

public class MonitorRepository : IMonitorRepository
{
    private readonly ApplicationDbContext _context;

    public MonitorRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Entities.Monitor?> GetByIdAsync(Guid monitorId, Guid organizationId)
    {
        return await _context.Monitors
            .Include(m => m.MonitorCategories)
            .Include(m => m.MonitorTags)
            .FirstOrDefaultAsync(m => m.MonitorId == monitorId && m.OrganizationId == organizationId);
    }

    public async Task<IEnumerable<Domain.Entities.Monitor>> GetAllByOrganizationAsync(Guid organizationId)
    {
        return await _context.Monitors
            .Include(m => m.MonitorCategories)
            .Include(m => m.MonitorTags)
            .Where(m => m.OrganizationId == organizationId)
            .ToListAsync();
    }

    public async Task<PagedResult<Domain.Entities.Monitor>> GetPagedAsync(Guid organizationId, MonitorFilter filter)
    {
        var query = _context.Monitors
            .Include(m => m.MonitorCategories)
            .Include(m => m.MonitorTags)
            .Where(m => m.OrganizationId == organizationId)
            .AsQueryable();

        // Search support (Name, Target, Description)
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchTerm = $"%{filter.Search.ToLower()}%";
            query = query.Where(m => EF.Functions.Like(m.Name.ToLower(), searchTerm) ||
                                     EF.Functions.Like(m.Target.ToLower(), searchTerm) ||
                                     EF.Functions.Like(m.Description.ToLower(), searchTerm));
        }

        // Filtering support
        if (filter.Type.HasValue)
        {
            query = query.Where(m => m.Type == filter.Type.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(m => m.Status == filter.Status.Value);
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(m => m.MonitorCategories.Any(c => c.CategoryId == filter.CategoryId.Value));
        }

        if (filter.TagId.HasValue)
        {
            query = query.Where(m => m.MonitorTags.Any(t => t.TagId == filter.TagId.Value));
        }

        if (filter.IsUp.HasValue)
        {
            query = query.Where(m => m.IsUp == filter.IsUp.Value);
        }

        // Sorting support
        query = filter.SortBy?.ToLower() switch
        {
            "name" => filter.SortDescending ? query.OrderByDescending(m => m.Name) : query.OrderBy(m => m.Name),
            "target" => filter.SortDescending ? query.OrderByDescending(m => m.Target) : query.OrderBy(m => m.Target),
            "status" => filter.SortDescending ? query.OrderByDescending(m => m.Status) : query.OrderBy(m => m.Status),
            _ => filter.SortDescending ? query.OrderByDescending(m => m.CreatedAt) : query.OrderBy(m => m.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<Domain.Entities.Monitor>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    public async Task<Domain.Entities.Monitor> CreateAsync(Domain.Entities.Monitor monitor)
    {
        _context.Monitors.Add(monitor);
        await _context.SaveChangesAsync();
        return monitor;
    }

    public async Task UpdateAsync(Domain.Entities.Monitor monitor)
    {
        _context.Monitors.Update(monitor);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid monitorId, Guid organizationId)
    {
        var monitor = await GetByIdAsync(monitorId, organizationId);
        if (monitor != null)
        {
            // Soft delete
            monitor.IsDeleted = true;
            monitor.DeletedAt = DateTime.UtcNow;
            _context.Monitors.Update(monitor);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Domain.Entities.Monitor?> GetByTargetAsync(string target, Guid organizationId)
    {
        return await _context.Monitors
            .FirstOrDefaultAsync(m => m.Target == target && m.OrganizationId == organizationId);
    }

    public async Task<IEnumerable<Domain.Entities.Monitor>> GetByCategoryAsync(Guid categoryId, Guid organizationId)
    {
        return await _context.Monitors
            .Where(m => m.OrganizationId == organizationId && m.MonitorCategories.Any(c => c.CategoryId == categoryId))
            .ToListAsync();
    }

    public async Task<IEnumerable<Domain.Entities.Monitor>> GetByTagAsync(Guid tagId, Guid organizationId)
    {
        return await _context.Monitors
            .Where(m => m.OrganizationId == organizationId && m.MonitorTags.Any(t => t.TagId == tagId))
            .ToListAsync();
    }

    public async Task<IEnumerable<MonitorCategory>> GetCategoriesAsync(Guid organizationId)
    {
        return await _context.MonitorCategories
            .Where(c => c.OrganizationId == organizationId)
            .ToListAsync();
    }

    public async Task<MonitorCategory> CreateCategoryAsync(MonitorCategory category)
    {
        _context.MonitorCategories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task UpdateCategoryAsync(MonitorCategory category)
    {
        _context.MonitorCategories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCategoryAsync(Guid categoryId, Guid organizationId)
    {
        var category = await _context.MonitorCategories
            .FirstOrDefaultAsync(c => c.CategoryId == categoryId && c.OrganizationId == organizationId);
        if (category != null)
        {
            _context.MonitorCategories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<MonitorTag>> GetTagsAsync(Guid organizationId)
    {
        return await _context.MonitorTags
            .Where(t => t.OrganizationId == organizationId)
            .ToListAsync();
    }

    public async Task<MonitorTag> CreateTagAsync(MonitorTag tag)
    {
        _context.MonitorTags.Add(tag);
        await _context.SaveChangesAsync();
        return tag;
    }

    public async Task DeleteTagAsync(Guid tagId, Guid organizationId)
    {
        var tag = await _context.MonitorTags
            .FirstOrDefaultAsync(t => t.TagId == tagId && t.OrganizationId == organizationId);
        if (tag != null)
        {
            _context.MonitorTags.Remove(tag);
            await _context.SaveChangesAsync();
        }
    }
}
