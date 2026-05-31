using MonitoringPlatform.Domain.Enums;

namespace MonitoringPlatform.Application.Features.Monitors.Models;

public class MonitorDto
{
    public Guid MonitorId { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MonitorType Type { get; set; }
    public string Target { get; set; } = string.Empty;
    public int? Port { get; set; }
    public int IntervalSeconds { get; set; }
    public int TimeoutSeconds { get; set; }
    public int? Retries { get; set; }
    public bool FollowRedirects { get; set; }
    public string? ExpectedStatusCode { get; set; }
    public string? ExpectedKeyword { get; set; }
    public string? HttpMethod { get; set; }
    public MonitorStatus Status { get; set; }
    public DateTime? LastCheckedAt { get; set; }
    public DateTime? LastDownAt { get; set; }
    public int? ResponseTimeMs { get; set; }
    public bool IsUp { get; set; }
    public int ConsecutiveFailures { get; set; }
    public int UptimePercentage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<MonitorCategoryDto> Categories { get; set; } = new();
    public List<MonitorTagDto> Tags { get; set; } = new();
}

public class MonitorCategoryDto
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

public class MonitorTagDto
{
    public Guid TagId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

public class PagedResponse<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}