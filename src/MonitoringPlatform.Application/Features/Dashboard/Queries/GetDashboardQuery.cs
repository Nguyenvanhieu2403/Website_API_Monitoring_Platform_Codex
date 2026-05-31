using MediatR;
using MonitoringPlatform.Application.Interfaces;
using MonitoringPlatform.Application.Models;
using MonitoringPlatform.Domain.Entities;

namespace MonitoringPlatform.Application.Features.Dashboard.Queries;

public class GetDashboardQuery : IRequest<Result<DashboardDto>>
{
    public Guid OrganizationId { get; set; }
    public string TimeRange { get; set; } = "24h"; // e.g., "24h", "7d", "30d"
}



