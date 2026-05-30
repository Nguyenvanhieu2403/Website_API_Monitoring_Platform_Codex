using MonitoringPlatform.Domain.Entities;

namespace MonitoringPlatform.Domain.Interfaces;

public interface IOrganizationRepository
{
    Task<Organization?> GetByIdAsync(Guid organizationId);
    Task<Organization?> GetBySlugAsync(string slug);
    Task<Organization> CreateAsync(Organization organization);
    Task UpdateAsync(Organization organization);
    Task DeleteAsync(Guid organizationId);
    Task<IEnumerable<Organization>> GetAllAsync();
}