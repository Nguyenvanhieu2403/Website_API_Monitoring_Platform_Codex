using MonitoringPlatform.Domain.Enums;

namespace MonitoringPlatform.Domain.Entities;

public class NotificationChannel
{
    public Guid ChannelId { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public NotificationChannelType Type { get; set; }
    public string Configuration { get; set; } = string.Empty; // JSON string for specific channel config (e.g., email address, webhook URL)
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Organization Organization { get; set; } = null!;
    public virtual ICollection<AlertRule> AlertRules { get; set; } = new List<AlertRule>();
}
