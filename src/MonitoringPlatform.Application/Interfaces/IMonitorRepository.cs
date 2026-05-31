using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Application.Features.Monitor.Queries.GetPagedMonitors;
using MonitoringPlatform.Application.Models;

namespace MonitoringPlatform.Application.Interfaces;

public interface IMonitorRepository
{
    Task<List<Domain.Entities.Monitor>> GetMonitorsByOrganizationIdAsync(Guid organizationId);
    Task<Domain.Entities.Monitor?> GetByIdAsync(Guid monitorId, Guid organizationId);
    Task<IEnumerable<Domain.Entities.Monitor>> GetAllByOrganizationAsync(Guid organizationId);
    Task<PagedResult<Domain.Entities.Monitor>> GetPagedAsync(Guid organizationId, MonitorFilter filter);
    Task<Domain.Entities.Monitor> CreateAsync(Domain.Entities.Monitor monitor);
    Task UpdateAsync(Domain.Entities.Monitor monitor);
    Task DeleteAsync(Guid monitorId, Guid organizationId);
    Task<Domain.Entities.Monitor?> GetByTargetAsync(string target, Guid organizationId);
    Task<IEnumerable<Domain.Entities.Monitor>> GetByCategoryAsync(Guid categoryId, Guid organizationId);
    Task<IEnumerable<Domain.Entities.Monitor>> GetByTagAsync(Guid tagId, Guid organizationId);
    Task<IEnumerable<MonitorCategory>> GetCategoriesAsync(Guid organizationId);
    Task<MonitorCategory> CreateCategoryAsync(MonitorCategory category);
    Task UpdateCategoryAsync(MonitorCategory category);
    Task DeleteCategoryAsync(Guid categoryId, Guid organizationId);
    Task<IEnumerable<MonitorTag>> GetTagsAsync(Guid organizationId);
    Task<MonitorTag> CreateTagAsync(MonitorTag tag);
    Task DeleteTagAsync(Guid tagId, Guid organizationId);
}
