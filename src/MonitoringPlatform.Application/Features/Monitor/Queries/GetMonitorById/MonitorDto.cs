using MonitoringPlatform.Domain.Enums;

namespace MonitoringPlatform.Application.Features.Monitor.Queries.GetMonitorById;

public class MonitorDto
{
    public Guid MonitorId { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public MonitorType Type { get; set; }
    public int Interval { get; set; }
    public int Timeout { get; set; }
    public MonitorStatus Status { get; set; }
    public bool IsUp { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastCheckedAt { get; set; }
    public DateTime? NextCheckAt { get; set; }
    public string? Headers { get; set; }
    public string? HttpMethod { get; set; }
    public string? ExpectedStatusCode { get; set; }
    public string? ExpectedContent { get; set; }
    public List<MonitorCategoryDto> MonitorCategories { get; set; } = new List<MonitorCategoryDto>();
    public List<MonitorTagDto> MonitorTags { get; set; } = new List<MonitorTagDto>();
}

public class MonitorCategoryDto
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class MonitorTagDto
{
    public Guid TagId { get; set; }
    public string Name { get; set; } = string.Empty;
}
