using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringPlatform.Application.Features.NotificationChannels.Commands;
using MonitoringPlatform.Application.Features.NotificationChannels.Models;
using MonitoringPlatform.Application.Features.NotificationChannels.Queries;
using System.Security.Claims;

namespace MonitoringPlatform.API.Controllers;

[Authorize]
[Route("api/organizations/{organizationId}/notification-channels")]
[ApiController]
public class NotificationChannelsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationChannelsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetCurrentOrganizationId()
    {
        var organizationIdClaim = User.FindFirst("organizationId")?.Value;
        if (Guid.TryParse(organizationIdClaim, out var organizationId))
        {
            return organizationId;
        }
        throw new UnauthorizedAccessException("Organization ID not found or invalid.");
    }

    private void ValidateOrganizationId(Guid organizationId)
    {
        if (GetCurrentOrganizationId() != organizationId)
        {
            throw new UnauthorizedAccessException("Access denied for this organization.");
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<NotificationChannelDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotificationChannels([FromRoute] Guid organizationId)
    {
        ValidateOrganizationId(organizationId);
        var query = new GetNotificationChannelsListQuery(organizationId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{channelId}")]
    [ProducesResponseType(typeof(NotificationChannelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNotificationChannelById([FromRoute] Guid organizationId, [FromRoute] Guid channelId)
    {
        ValidateOrganizationId(organizationId);
        var query = new GetNotificationChannelByIdQuery(channelId, organizationId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(NotificationChannelDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateNotificationChannel([FromRoute] Guid organizationId, [FromBody] CreateNotificationChannelCommand command)
    {
        ValidateOrganizationId(organizationId);
        if (organizationId != command.OrganizationId) return BadRequest("Organization ID in route and body must match.");

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetNotificationChannelById), new { organizationId, channelId = result.ChannelId }, result);
    }

    [HttpPut("{channelId}")]
    [ProducesResponseType(typeof(NotificationChannelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateNotificationChannel([FromRoute] Guid organizationId, [FromRoute] Guid channelId, [FromBody] UpdateNotificationChannelCommand command)
    {
        ValidateOrganizationId(organizationId);
        if (channelId != command.ChannelId) return BadRequest("Channel ID in route and body must match.");
        if (organizationId != command.OrganizationId) return BadRequest("Organization ID in route and body must match.");

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{channelId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNotificationChannel([FromRoute] Guid organizationId, [FromRoute] Guid channelId)
    {
        ValidateOrganizationId(organizationId);
        var command = new DeleteNotificationChannelCommand(channelId, organizationId);
        await _mediator.Send(command);
        return NoContent();
    }
}
