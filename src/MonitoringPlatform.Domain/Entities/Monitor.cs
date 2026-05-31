using MonitoringPlatform.Domain.Enums;

namespace MonitoringPlatform.Domain.Entities;

public class Monitor
{
    public Guid MonitorId { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MonitorType Type { get; set; }
    public string Target { get; set; } = string.Empty; // URL, IP, or hostname
    public int? Port { get; set; }
    public int IntervalSeconds { get; set; } = 60;
    public int TimeoutSeconds { get; set; } = 30;
    public int? Retries { get; set; } = 3;
    public bool FollowRedirects { get; set; } = true;
    public string? ExpectedStatusCode { get; set; }
    public string? ExpectedKeyword { get; set; }
    public string? RequestBody { get; set; }
    public string? RequestHeaders { get; set; }
    public string? HttpMethod { get; set; } = "GET";
    public MonitorStatus Status { get; set; } = MonitorStatus.Active;
    public DateTime? LastCheckedAt { get; set; }
    public DateTime? LastDownAt { get; set; }
    public int? ResponseTimeMs { get; set; }
    public bool IsUp { get; set; } = true;
    public int ConsecutiveFailures { get; set; }
    public int UptimePercentage { get; set; } = 100;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Organization Organization { get; set; } = null!;
    public virtual ICollection<MonitorCategory> MonitorCategories { get; set; } = new List<MonitorCategory>();
    public virtual ICollection<MonitorTag> MonitorTags { get; set; } = new List<MonitorTag>();
    public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    public virtual ICollection<MonitorLog> MonitorLogs { get; set; } = new List<MonitorLog>();
}
