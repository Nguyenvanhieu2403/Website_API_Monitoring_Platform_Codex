namespace MonitoringPlatform.Domain.Entities;

public class MonitorCategory
{
    public Guid CategoryId { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = "#3B82F6";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Organization Organization { get; set; } = null!;
    public virtual ICollection<Monitor> Monitors { get; set; } = new List<Monitor>();
}