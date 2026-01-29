using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockMind.Application.Commands.Alerts;
using StockMind.Application.Queries.Alerts;

namespace StockMind.API.Controllers;

[ApiController]
[Route("api/stock-alerts")]
[Authorize]
public class StockAlertsController : BaseController
{
    private readonly IMediator _mediator;

    public StockAlertsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all active stock alerts
    /// </summary>
    [HttpGet("active")]
    [Authorize(Roles = "Admin,Manager,Operator,Viewer")]
    public async Task<IActionResult> GetActiveAlerts()
    {
        var query = new GetActiveAlertsQuery();
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Acknowledge a stock alert
    /// </summary>
    [HttpPost("{id}/acknowledge")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AcknowledgeAlert(
        Guid id,
        [FromBody] AcknowledgeAlertRequest request)
    {
        var command = new AcknowledgeAlertCommand(id, request.Notes);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Alert acknowledged successfully" });
    }
}

public record AcknowledgeAlertRequest(string? Notes);
