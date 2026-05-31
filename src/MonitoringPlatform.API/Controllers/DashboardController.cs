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
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetOrganizationId()
    {
        var orgIdClaim = User.FindFirst("organization_id")?.Value;
        if (string.IsNullOrEmpty(orgIdClaim) || !Guid.TryParse(orgIdClaim, out var organizationId))
        {
            throw new UnauthorizedAccessException("Organization ID claim is missing or invalid.");
        }
        return organizationId;
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

        if (!result.IsSuccess)
        {
            return BadRequest(new ApiResponse<AggregatedAnalyticsDto>
            {
                Success = false,
                StatusCode = 400,
                Message = result.Error
            });
        }

        return Ok(new ApiResponse<AggregatedAnalyticsDto>
        {
            Success = true,
            StatusCode = 200,
            Message = "Lấy dữ liệu thành công.",
            Data = result.Value
        });
    }
}
