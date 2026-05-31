using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Domain.Enums;

namespace MonitoringPlatform.Domain.Interfaces;

public interface IMonitorRepository
{
    Task<Entities.Monitor?> GetByIdAsync(Guid monitorId, Guid organizationId);
    Task<IEnumerable<Entities.Monitor>> GetAllByOrganizationAsync(Guid organizationId);
    Task<PagedResult<Entities.Monitor>> GetPagedAsync(Guid organizationId, MonitorFilter filter);
    Task<Entities.Monitor> CreateAsync(Entities.Monitor monitor);
    Task UpdateAsync(Entities.Monitor monitor);
    Task DeleteAsync(Guid monitorId, Guid organizationId);
    Task<Entities.Monitor?> GetByTargetAsync(string target, Guid organizationId);
    Task<IEnumerable<Entities.Monitor>> GetByCategoryAsync(Guid categoryId, Guid organizationId);
    Task<IEnumerable<Entities.Monitor>> GetByTagAsync(Guid tagId, Guid organizationId);
    Task<IEnumerable<MonitorCategory>> GetCategoriesAsync(Guid organizationId);
    Task<MonitorCategory> CreateCategoryAsync(MonitorCategory category);
    Task UpdateCategoryAsync(MonitorCategory category);
    Task DeleteCategoryAsync(Guid categoryId, Guid organizationId);
    Task<IEnumerable<MonitorTag>> GetTagsAsync(Guid organizationId);
    Task<MonitorTag> CreateTagAsync(MonitorTag tag);
    Task DeleteTagAsync(Guid tagId, Guid organizationId);
}

public class MonitorFilter
{
    public string? Search { get; set; }
    public Enums.MonitorType? Type { get; set; }
    public Enums.MonitorStatus? Status { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? TagId { get; set; }
    public bool? IsUp { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}