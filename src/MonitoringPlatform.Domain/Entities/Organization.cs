using MonitoringPlatform.Domain.Enums;

namespace MonitoringPlatform.Domain.Entities;

public class Organization
{
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public PlanType PlanType { get; set; }
    public OrganizationStatus Status { get; set; }
    public int MaxMonitors { get; set; }
    public int MaxAlerts { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}