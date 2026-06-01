using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringPlatform.Application.Features.Dashboard.Queries;
using MonitoringPlatform.Application.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace MonitoringPlatform.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/dashboard")]
[Produces("application/json")]
public class DashboardController : BaseApiController
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get aggregated analytics for the dashboard
    /// </summary>
    [HttpGet("summary")]
    [SwaggerOperation(Summary = "Get aggregated analytics for the dashboard")]
    [SwaggerResponse(200, "Analytics retrieved successfully", typeof(ApiResponse<AggregatedAnalyticsDto>))]
    [SwaggerResponse(401, "Unauthorized", typeof(ApiResponse<object>))]
    public async Task<ActionResult<ApiResponse<AggregatedAnalyticsDto>>> GetSummary([FromQuery] string timeRange = "24h")
    {
        var organizationId = GetOrganizationId();
        var query = new GetAggregatedAnalyticsQuery
        {
            OrganizationId = organizationId,
            TimeRange = timeRange
        };

        var result = await _mediator.Send(query);
        return HandleResult(result, StatusCodes.Status200OK, "Lấy dữ liệu thành công.");
    }
}
