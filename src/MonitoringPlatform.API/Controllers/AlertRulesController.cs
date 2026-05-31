using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringPlatform.Application.Features.AlertRules.Commands;
using MonitoringPlatform.Application.Features.AlertRules.Models;
using MonitoringPlatform.Application.Features.AlertRules.Queries;
using System.Security.Claims;

namespace MonitoringPlatform.API.Controllers;

[Authorize]
[Route("api/organizations/{organizationId}/monitors/{monitorId}/alert-rules")]
[ApiController]
public class AlertRulesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AlertRulesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetCurrentOrganizationId()
    {
        var organizationIdClaim = User.FindFirst("organization_id")?.Value;
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
    [ProducesResponseType(typeof(IEnumerable<AlertRuleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAlertRules([FromRoute] Guid organizationId, [FromRoute] Guid monitorId)
    {
        ValidateOrganizationId(organizationId);
        var query = new GetAlertRulesListQuery(monitorId, organizationId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{ruleId}")]
    [ProducesResponseType(typeof(AlertRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAlertRuleById([FromRoute] Guid organizationId, [FromRoute] Guid ruleId)
    {
        ValidateOrganizationId(organizationId);
        var query = new GetAlertRuleByIdQuery(ruleId, organizationId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AlertRuleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAlertRule([FromRoute] Guid organizationId, [FromRoute] Guid monitorId, [FromBody] CreateAlertRuleCommand command)
    {
        ValidateOrganizationId(organizationId);
        if (monitorId != command.MonitorId) return BadRequest("Monitor ID in route and body must match.");
        if (organizationId != command.OrganizationId) return BadRequest("Organization ID in route and body must match.");

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAlertRuleById), new { organizationId, monitorId, ruleId = result.RuleId }, result);
    }

    [HttpPut("{ruleId}")]
    [ProducesResponseType(typeof(AlertRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAlertRule([FromRoute] Guid organizationId, [FromRoute] Guid monitorId, [FromRoute] Guid ruleId, [FromBody] UpdateAlertRuleCommand command)
    {
        ValidateOrganizationId(organizationId);
        if (ruleId != command.RuleId) return BadRequest("Alert Rule ID in route and body must match.");
        if (monitorId != command.MonitorId) return BadRequest("Monitor ID in route and body must match.");
        if (organizationId != command.OrganizationId) return BadRequest("Organization ID in route and body must match.");

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{ruleId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAlertRule([FromRoute] Guid organizationId, [FromRoute] Guid ruleId)
    {
        ValidateOrganizationId(organizationId);
        var command = new DeleteAlertRuleCommand(ruleId, organizationId);
        await _mediator.Send(command);
        return NoContent();
    }
}
