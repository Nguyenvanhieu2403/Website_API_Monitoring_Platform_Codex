namespace MonitoringPlatform.Application.Features.Monitor.Queries.GetPagedMonitors;

using MonitoringPlatform.Domain.Enums;

public class MonitorFilter
{
    public string? Search { get; set; }
    public MonitorType? Type { get; set; }
    public MonitorStatus? Status { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? TagId { get; set; }
    public bool? IsUp { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
