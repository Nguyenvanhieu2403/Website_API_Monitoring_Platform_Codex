using MediatR;
using MonitoringPlatform.Application.Interfaces;
using MonitoringPlatform.Application.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MonitoringPlatform.Application.Features.Dashboard.Queries;

public class GetAggregatedAnalyticsQuery : IRequest<Result<AggregatedAnalyticsDto>>
{
    public Guid OrganizationId { get; set; }
    public string TimeRange { get; set; } = "24h"; // e.g., "24h", "7d", "30d"
}

public class GetAggregatedAnalyticsQueryHandler : IRequestHandler<GetAggregatedAnalyticsQuery, Result<AggregatedAnalyticsDto>>
{
    private readonly IMonitorRepository _monitorRepository;
    private readonly IMetricRepository _metricRepository;

    public GetAggregatedAnalyticsQueryHandler(IMonitorRepository monitorRepository, IMetricRepository metricRepository)
    {
        _monitorRepository = monitorRepository;
        _metricRepository = metricRepository;
    }

    public async Task<Result<AggregatedAnalyticsDto>> Handle(GetAggregatedAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var monitors = await _monitorRepository.GetMonitorsByOrganizationIdAsync(request.OrganizationId);

        var allMetrics = new List<MonitorMetricDto>();
        foreach (var monitor in monitors)
        {
            var metrics = await _metricRepository.GetMetricsForMonitorInTimeRangeAsync(
                monitor.MonitorId, request.TimeRange);
            allMetrics.AddRange(metrics);
        }

        var aggregatedAnalyticsDto = new AggregatedAnalyticsDto
        {
            OrganizationId = request.OrganizationId,
            TotalMonitors = monitors.Count,
            TotalChecks = allMetrics.Count,
            AverageUptimePercentage = CalculateOverallUptimePercentage(allMetrics),
            AverageResponseTime = CalculateOverallAverageResponseTime(allMetrics),
            AverageFailureRate = CalculateOverallFailureRate(allMetrics)
        };

        return Result<AggregatedAnalyticsDto>.Success(aggregatedAnalyticsDto);
    }

    private static decimal CalculateOverallUptimePercentage(IEnumerable<MonitorMetricDto> metrics)
    {
        if (!metrics.Any()) return 100m;

        var totalChecks = metrics.Count();
        var successfulChecks = metrics.Count(m => m.Status == "Up");

        return (decimal)successfulChecks / totalChecks * 100m;
    }

    private static int CalculateOverallAverageResponseTime(IEnumerable<MonitorMetricDto> metrics)
    {
        if (!metrics.Any()) return 0;

        var responseTimes = metrics.Where(m => m.Status == "Up").Select(m => m.ResponseTime).ToList();

        return responseTimes.Any() ? (int)responseTimes.Average() : 0;
    }

    private static decimal CalculateOverallFailureRate(IEnumerable<MonitorMetricDto> metrics)
    {
        if (!metrics.Any()) return 0m;

        var totalChecks = metrics.Count();
        var failedChecks = metrics.Count(m => m.Status == "Down");

        return (decimal)failedChecks / totalChecks * 100m;
    }
}
