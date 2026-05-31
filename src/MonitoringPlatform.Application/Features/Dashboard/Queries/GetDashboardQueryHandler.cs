using MediatR;
using MonitoringPlatform.Application.Interfaces;
using MonitoringPlatform.Application.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MonitoringPlatform.Application.Features.Dashboard.Queries;

public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, Result<DashboardDto>>
{
    private readonly IMonitorRepository _monitorRepository;
    private readonly IMetricRepository _metricRepository;

    public GetDashboardQueryHandler(IMonitorRepository monitorRepository, IMetricRepository metricRepository)
    {
        _monitorRepository = monitorRepository;
        _metricRepository = metricRepository;
    }

    public async Task<Result<DashboardDto>> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        // Fetch monitors for the organization.
        var monitors = await _monitorRepository.GetMonitorsByOrganizationIdAsync(request.OrganizationId);

        var dashboardDto = new DashboardDto
        {
            OrganizationId = request.OrganizationId,
            MonitorOverviews = new List<MonitorOverviewDto>()
        };

        foreach (var monitor in monitors)
        {
            // Fetch metrics for each monitor within the specified time range.
            var metrics = await _metricRepository.GetMetricsForMonitorInTimeRangeAsync(
                monitor.MonitorId, request.TimeRange);

            // Calculate statistics for each monitor.
            var uptimePercentage = CalculateUptimePercentage(metrics);
            var responseTimeMetrics = CalculateResponseTimeMetrics(metrics);
            var failureRate = CalculateFailureRate(metrics);
            var statusTimeline = GenerateStatusTimeline(metrics);

            dashboardDto.MonitorOverviews.Add(new MonitorOverviewDto
            {
                MonitorId = monitor.MonitorId,
                MonitorName = monitor.Name,
                UptimePercentage = uptimePercentage,
                AverageResponseTime = responseTimeMetrics.Average,
                MinResponseTime = responseTimeMetrics.Min,
                MaxResponseTime = responseTimeMetrics.Max,
                FailureRate = failureRate,
                StatusTimeline = statusTimeline
            });
        }

        return Result<DashboardDto>.Success(dashboardDto);
    }

    private static decimal CalculateUptimePercentage(IEnumerable<MonitorMetricDto> metrics)
    {
        if (!metrics.Any()) return 100m;

        var totalChecks = metrics.Count();
        var successfulChecks = metrics.Count(m => m.Status == "Up");

        return (decimal)successfulChecks / totalChecks * 100m;
    }

    private static ResponseTimeMetricsDto CalculateResponseTimeMetrics(IEnumerable<MonitorMetricDto> metrics)
    {
        if (!metrics.Any())
        {
            return new ResponseTimeMetricsDto { Average = 0, Min = 0, Max = 0 };
        }

        var responseTimes = metrics.Where(m => m.Status == "Up").Select(m => m.ResponseTime).ToList();

        if (!responseTimes.Any())
        {
            return new ResponseTimeMetricsDto { Average = 0, Min = 0, Max = 0 };
        }

        return new ResponseTimeMetricsDto
        {
            Average = (int)responseTimes.Average(),
            Min = (int)responseTimes.Min(),
            Max = (int)responseTimes.Max()
        };
    }

    private static decimal CalculateFailureRate(IEnumerable<MonitorMetricDto> metrics)
    {
        if (!metrics.Any()) return 0m;

        var totalChecks = metrics.Count();
        var failedChecks = metrics.Count(m => m.Status == "Down");

        return (decimal)failedChecks / totalChecks * 100m;
    }

    private static List<MonitorStatusTimelineDto> GenerateStatusTimeline(IEnumerable<MonitorMetricDto> metrics)
    {
        return metrics
            .OrderBy(m => m.Timestamp)
            .Select(m => new MonitorStatusTimelineDto
            {
                Timestamp = m.Timestamp,
                Status = m.Status
            })
            .ToList();
    }
}