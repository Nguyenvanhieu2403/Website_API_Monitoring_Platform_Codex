using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringPlatform.Application.Features.AlertRules.Commands;
using MonitoringPlatform.Application.Features.AlertRules.Models;
using MonitoringPlatform.Application.Features.AlertRules.Queries;
using MonitoringPlatform.Application.Models;
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

    private ActionResult<ApiResponse<T>> HandleResult<T>(Result<T> result, int successStatusCode, string successMessage)
    {
        if (!result.IsSuccess)
        {
            var statusCode = 400; // Default for bad request
            if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase) || result.Error.Contains("không tìm thấy", StringComparison.OrdinalIgnoreCase))
            {
                statusCode = 404; // Not Found
            }
            else if (result.Error.Contains("access denied", StringComparison.OrdinalIgnoreCase) || result.Error.Contains("unauthorized", StringComparison.OrdinalIgnoreCase))
            {
                statusCode = 401; // Unauthorized
            }

            return StatusCode(statusCode, new ApiResponse<T>
            {
                Success = false,
                StatusCode = statusCode,
                Message = result.Error,
                Errors = result.Errors
            });
        }

        return StatusCode(successStatusCode, new ApiResponse<T>
        {
            Success = true,
            StatusCode = successStatusCode,
            Message = successMessage,
            Data = result.Value
        });
    }

    private IActionResult HandleResult(Result result, int successStatusCode, string successMessage)
    {
        if (!result.IsSuccess)
        {
            var statusCode = 400; // Default for bad request
            if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase) || result.Error.Contains("không tìm thấy", StringComparison.OrdinalIgnoreCase))
            {
                statusCode = 404; // Not Found
            }
            else if (result.Error.Contains("access denied", StringComparison.OrdinalIgnoreCase) || result.Error.Contains("unauthorized", StringComparison.OrdinalIgnoreCase))
            {
                statusCode = 401; // Unauthorized
            }

            return StatusCode(statusCode, new ApiResponse<object>
            {
                Success = false,
                StatusCode = statusCode,
                Message = result.Error,
                Errors = result.Errors
            });
        }

        return StatusCode(successStatusCode, new ApiResponse<object>
        {
            Success = true,
            StatusCode = successStatusCode,
            Message = successMessage
        });
    }

    private void ValidateOrganizationId(Guid organizationId)
    {
        if (GetCurrentOrganizationId() != organizationId)
        {
            throw new UnauthorizedAccessException("Access denied for this organization.");
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AlertRuleDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<IEnumerable<AlertRuleDto>>>> GetAlertRules([FromRoute] Guid organizationId, [FromRoute] Guid monitorId)
    {
        ValidateOrganizationId(organizationId);
        var query = new GetAlertRulesListQuery(monitorId, organizationId);
        var result = await _mediator.Send(query);
        return HandleResult(result, StatusCodes.Status200OK, "Lấy danh sách quy tắc cảnh báo thành công.");
    }

    [HttpGet("{ruleId}")]
    [ProducesResponseType(typeof(ApiResponse<AlertRuleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AlertRuleDto>>> GetAlertRuleById([FromRoute] Guid organizationId, [FromRoute] Guid ruleId)
    {
        ValidateOrganizationId(organizationId);
        var query = new GetAlertRuleByIdQuery(ruleId, organizationId);
        var result = await _mediator.Send(query);
        return HandleResult(result, StatusCodes.Status200OK, "Lấy quy tắc cảnh báo thành công.");
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AlertRuleDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AlertRuleDto>>> CreateAlertRule([FromRoute] Guid organizationId, [FromRoute] Guid monitorId, [FromBody] CreateAlertRuleCommand command)
    {
        ValidateOrganizationId(organizationId);
        if (monitorId != command.MonitorId) return BadRequest(new ApiResponse<object> { Success = false, StatusCode = 400, Message = "ID Monitor trong route và body phải khớp." });
        if (organizationId != command.OrganizationId) return BadRequest(new ApiResponse<object> { Success = false, StatusCode = 400, Message = "ID Tổ chức trong route và body phải khớp." });

        var result = await _mediator.Send(command);
        return HandleResult(result, StatusCodes.Status201Created, "Tạo quy tắc cảnh báo thành công.");
    }

    [HttpPut("{ruleId}")]
    [ProducesResponseType(typeof(ApiResponse<AlertRuleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AlertRuleDto>>> UpdateAlertRule([FromRoute] Guid organizationId, [FromRoute] Guid monitorId, [FromRoute] Guid ruleId, [FromBody] UpdateAlertRuleCommand command)
    {
        ValidateOrganizationId(organizationId);
        if (ruleId != command.RuleId) return BadRequest(new ApiResponse<object> { Success = false, StatusCode = 400, Message = "ID Quy tắc cảnh báo trong route và body phải khớp." });
        if (monitorId != command.MonitorId) return BadRequest(new ApiResponse<object> { Success = false, StatusCode = 400, Message = "ID Monitor trong route và body phải khớp." });
        if (organizationId != command.OrganizationId) return BadRequest(new ApiResponse<object> { Success = false, StatusCode = 400, Message = "ID Tổ chức trong route và body phải khớp." });

        var result = await _mediator.Send(command);
        return HandleResult(result, StatusCodes.Status200OK, "Cập nhật quy tắc cảnh báo thành công.");
    }

    [HttpDelete("{ruleId}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAlertRule([FromRoute] Guid organizationId, [FromRoute] Guid ruleId)
    {
        ValidateOrganizationId(organizationId);
        var command = new DeleteAlertRuleCommand(ruleId, organizationId);
        var result = await _mediator.Send(command);
        return HandleResult(result, StatusCodes.Status200OK, "Xóa quy tắc cảnh báo thành công.");
    }
}
