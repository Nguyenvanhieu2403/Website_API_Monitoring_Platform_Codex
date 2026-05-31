namespace MonitoringPlatform.Domain.Entities;

public class MonitorTag
{
    public Guid TagId { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#10B981";
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual Organization Organization { get; set; } = null!;
    public virtual ICollection<Monitor> Monitors { get; set; } = new List<Monitor>();
}