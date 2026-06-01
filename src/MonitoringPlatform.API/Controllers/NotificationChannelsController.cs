using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringPlatform.Application.Features.NotificationChannels.Commands;
using MonitoringPlatform.Application.Features.NotificationChannels.Models;
using MonitoringPlatform.Application.Features.NotificationChannels.Queries;
using MonitoringPlatform.Application.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace MonitoringPlatform.API.Controllers;

[Authorize]
[Route("api/organizations/{organizationId}/notification-channels")]
[ApiController]
public class NotificationChannelsController : BaseApiController
{
    private readonly IMediator _mediator;

    public NotificationChannelsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Lấy danh sách kênh thông báo")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<NotificationChannelDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<NotificationChannelDto>>>> GetNotificationChannels([FromRoute] Guid organizationId)
    {
        ValidateOrganizationId(organizationId);
        var query = new GetNotificationChannelsListQuery(organizationId);
        var result = await _mediator.Send(query);
        return HandleResult(result, StatusCodes.Status200OK, "Lấy danh sách kênh thông báo thành công.");
    }

    [HttpGet("{channelId}")]
    [SwaggerOperation(Summary = "Lấy kênh thông báo theo ID")]
    [ProducesResponseType(typeof(ApiResponse<NotificationChannelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<NotificationChannelDto>>> GetNotificationChannelById([FromRoute] Guid organizationId, [FromRoute] Guid channelId)
    {
        ValidateOrganizationId(organizationId);
        var query = new GetNotificationChannelByIdQuery(channelId, organizationId);
        var result = await _mediator.Send(query);
        return HandleResult(result, StatusCodes.Status200OK, "Lấy kênh thông báo thành công.");
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Tạo kênh thông báo mới")]
    [ProducesResponseType(typeof(ApiResponse<NotificationChannelDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<NotificationChannelDto>>> CreateNotificationChannel([FromRoute] Guid organizationId, [FromBody] CreateNotificationChannelCommand command)
    {
        ValidateOrganizationId(organizationId);
        if (organizationId != command.OrganizationId)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                StatusCode = 400,
                Message = "ID Tổ chức trong route và body phải khớp.",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }

        var result = await _mediator.Send(command);
        return HandleResult(result, StatusCodes.Status201Created, "Tạo kênh thông báo thành công.");
    }

    [HttpPut("{channelId}")]
    [SwaggerOperation(Summary = "Cập nhật kênh thông báo")]
    [ProducesResponseType(typeof(ApiResponse<NotificationChannelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<NotificationChannelDto>>> UpdateNotificationChannel([FromRoute] Guid organizationId, [FromRoute] Guid channelId, [FromBody] UpdateNotificationChannelCommand command)
    {
        ValidateOrganizationId(organizationId);
        if (channelId != command.ChannelId)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                StatusCode = 400,
                Message = "ID Kênh trong route và body phải khớp.",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
        if (organizationId != command.OrganizationId)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                StatusCode = 400,
                Message = "ID Tổ chức trong route và body phải khớp.",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }

        var result = await _mediator.Send(command);
        return HandleResult(result, StatusCodes.Status200OK, "Cập nhật kênh thông báo thành công.");
    }

    [HttpDelete("{channelId}")]
    [SwaggerOperation(Summary = "Xóa kênh thông báo")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNotificationChannel([FromRoute] Guid organizationId, [FromRoute] Guid channelId)
    {
        ValidateOrganizationId(organizationId);
        var command = new DeleteNotificationChannelCommand(channelId, organizationId);
        var result = await _mediator.Send(command);
        return HandleResult(result, StatusCodes.Status200OK, "Xóa kênh thông báo thành công.");
    }
}
