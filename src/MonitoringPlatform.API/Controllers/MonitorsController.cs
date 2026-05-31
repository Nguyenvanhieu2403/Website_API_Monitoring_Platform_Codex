using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringPlatform.Application.Features.Monitors.Commands;
using MonitoringPlatform.Application.Features.Monitors.Models;
using MonitoringPlatform.Application.Features.Monitors.Queries;
using MonitoringPlatform.Application.Models;
using MonitoringPlatform.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace MonitoringPlatform.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/monitors")]
[Produces("application/json")]
public class MonitorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MonitorsController(IMediator mediator)
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
    /// Create a new monitor
    /// </summary>
    [HttpPost]
    [SwaggerOperation(Summary = "Create a new monitor")]
    [SwaggerResponse(201, "Monitor created successfully", typeof(ApiResponse<MonitorDto>))]
    [SwaggerResponse(400, "Invalid input data", typeof(ApiResponse<object>))]
    [SwaggerResponse(401, "Unauthorized", typeof(ApiResponse<object>))]
    public async Task<ActionResult<ApiResponse<MonitorDto>>> Create([FromBody] CreateMonitorRequest request)
    {
        var organizationId = GetOrganizationId();
        var command = new CreateMonitorCommand
        {
            OrganizationId = organizationId,
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            Target = request.Target,
            Port = request.Port,
            IntervalSeconds = request.IntervalSeconds,
            TimeoutSeconds = request.TimeoutSeconds,
            Retries = request.Retries,
            FollowRedirects = request.FollowRedirects,
            ExpectedStatusCode = request.ExpectedStatusCode,
            ExpectedKeyword = request.ExpectedKeyword,
            HttpMethod = request.HttpMethod,
            CategoryIds = request.CategoryIds,
            TagIds = request.TagIds
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new ApiResponse<MonitorDto>
            {
                Success = false,
                StatusCode = 400,
                Message = result.Error,
                Errors = result.Errors
            });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.MonitorId }, new ApiResponse<MonitorDto>
        {
            Success = true,
            StatusCode = 201,
            Message = "Tạo monitor thành công.",
            Data = result.Value
        });
    }

    /// <summary>
    /// Update an existing monitor
    /// </summary>
    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Update an existing monitor")]
    [SwaggerResponse(200, "Monitor updated successfully", typeof(ApiResponse<MonitorDto>))]
    [SwaggerResponse(400, "Invalid input data", typeof(ApiResponse<object>))]
    [SwaggerResponse(401, "Unauthorized", typeof(ApiResponse<object>))]
    [SwaggerResponse(404, "Monitor not found", typeof(ApiResponse<object>))]
    public async Task<ActionResult<ApiResponse<MonitorDto>>> Update(Guid id, [FromBody] UpdateMonitorRequest request)
    {
        var organizationId = GetOrganizationId();
        var command = new UpdateMonitorCommand
        {
            MonitorId = id,
            OrganizationId = organizationId,
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            Target = request.Target,
            Port = request.Port,
            IntervalSeconds = request.IntervalSeconds,
            TimeoutSeconds = request.TimeoutSeconds,
            Retries = request.Retries,
            FollowRedirects = request.FollowRedirects,
            ExpectedStatusCode = request.ExpectedStatusCode,
            ExpectedKeyword = request.ExpectedKeyword,
            HttpMethod = request.HttpMethod,
            CategoryIds = request.CategoryIds,
            TagIds = request.TagIds
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            var statusCode = result.Error.Contains("không tìm thấy", StringComparison.OrdinalIgnoreCase) ? 404 : 400;
            return StatusCode(statusCode, new ApiResponse<MonitorDto>
            {
                Success = false,
                StatusCode = statusCode,
                Message = result.Error,
                Errors = result.Errors
            });
        }

        return Ok(new ApiResponse<MonitorDto>
        {
            Success = true,
            StatusCode = 200,
            Message = "Cập nhật monitor thành công.",
            Data = result.Value
        });
    }

    /// <summary>
    /// Soft delete a monitor
    /// </summary>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Delete (soft delete) a monitor")]
    [SwaggerResponse(200, "Monitor deleted successfully", typeof(ApiResponse<object>))]
    [SwaggerResponse(401, "Unauthorized", typeof(ApiResponse<object>))]
    [SwaggerResponse(404, "Monitor not found", typeof(ApiResponse<object>))]
    public async Task<IActionResult> Delete(Guid id)
    {
        var organizationId = GetOrganizationId();
        var command = new DeleteMonitorCommand
        {
            MonitorId = id,
            OrganizationId = organizationId
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            var statusCode = result.Error.Contains("không tìm thấy", StringComparison.OrdinalIgnoreCase) ? 404 : 400;
            return StatusCode(statusCode, new ApiResponse<object>
            {
                Success = false,
                StatusCode = statusCode,
                Message = result.Error
            });
        }

        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = 200,
            Message = "Xóa monitor thành công."
        });
    }

    /// <summary>
    /// Get monitor by its ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Get monitor by ID")]
    [SwaggerResponse(200, "Monitor details retrieved successfully", typeof(ApiResponse<MonitorDto>))]
    [SwaggerResponse(401, "Unauthorized", typeof(ApiResponse<object>))]
    [SwaggerResponse(404, "Monitor not found", typeof(ApiResponse<object>))]
    public async Task<ActionResult<ApiResponse<MonitorDto>>> GetById(Guid id)
    {
        var organizationId = GetOrganizationId();
        var query = new GetMonitorByIdQuery
        {
            MonitorId = id,
            OrganizationId = organizationId
        };

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(new ApiResponse<MonitorDto>
            {
                Success = false,
                StatusCode = 404,
                Message = result.Error
            });
        }

        return Ok(new ApiResponse<MonitorDto>
        {
            Success = true,
            StatusCode = 200,
            Message = "Lấy dữ liệu thành công.",
            Data = result.Value
        });
    }

    /// <summary>
    /// Get paged list of monitors with filtering and search support
    /// </summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Get paged list of monitors")]
    [SwaggerResponse(200, "Monitors retrieved successfully", typeof(ApiResponse<PagedResponse<MonitorDto>>))]
    [SwaggerResponse(401, "Unauthorized", typeof(ApiResponse<object>))]
    public async Task<ActionResult<ApiResponse<PagedResponse<MonitorDto>>>> GetList([FromQuery] GetMonitorsListRequest request)
    {
        var organizationId = GetOrganizationId();
        var query = new GetMonitorsListQuery
        {
            OrganizationId = organizationId,
            Search = request.Search,
            Type = request.Type,
            Status = request.Status,
            CategoryId = request.CategoryId,
            TagId = request.TagId,
            IsUp = request.IsUp,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending
        };

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new ApiResponse<PagedResponse<MonitorDto>>
            {
                Success = false,
                StatusCode = 400,
                Message = result.Error
            });
        }

        return Ok(new ApiResponse<PagedResponse<MonitorDto>>
        {
            Success = true,
            StatusCode = 200,
            Message = "Lấy dữ liệu thành công.",
            Data = result.Value
        });
    }
}

public class CreateMonitorRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MonitorType Type { get; set; }
    public string Target { get; set; } = string.Empty;
    public int? Port { get; set; }
    public int IntervalSeconds { get; set; } = 60;
    public int TimeoutSeconds { get; set; } = 30;
    public int? Retries { get; set; } = 3;
    public bool FollowRedirects { get; set; } = true;
    public string? ExpectedStatusCode { get; set; }
    public string? ExpectedKeyword { get; set; }
    public string? HttpMethod { get; set; } = "GET";
    public List<Guid> CategoryIds { get; set; } = new();
    public List<Guid> TagIds { get; set; } = new();
}

public class UpdateMonitorRequest
{
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
    public List<Guid> CategoryIds { get; set; } = new();
    public List<Guid> TagIds { get; set; } = new();
}

public class GetMonitorsListRequest
{
    public string? Search { get; set; }
    public MonitorType? Type { get; set; }
    public MonitorStatus? Status { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? TagId { get; set; }
    public bool? IsUp { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}
