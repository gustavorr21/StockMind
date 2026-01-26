using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockMind.Application.Commands.Stock;
using StockMind.Application.Queries.Stock;

namespace StockMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all endpoints
public class StockController : BaseController
{
    private readonly IMediator _mediator;

    public StockController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("product/{productId:guid}")]
    [Authorize(Roles = "Admin,Manager,Operator,Viewer")] // All roles can view
    public async Task<IActionResult> GetByProductId(Guid productId)
    {
        var query = new GetStockByProductIdQuery(productId);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Data);
    }

    [HttpGet("low-stock")]
    [Authorize(Roles = "Admin,Manager,Operator")] // Viewer cannot see alerts
    public async Task<IActionResult> GetLowStock()
    {
        var query = new GetLowStockProductsQuery();
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Data);
    }

    [HttpPost("add")]
    [Authorize(Roles = "Admin,Manager,Operator")] // Only Admin, Manager and Operator can add stock
    public async Task<IActionResult> AddStock([FromBody] AddStockCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new { success = true, message = "Stock added successfully" });
    }

    [HttpPost("remove")]
    [Authorize(Roles = "Admin,Manager,Operator")] // Only Admin, Manager and Operator can remove stock
    public async Task<IActionResult> RemoveStock([FromBody] RemoveStockCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new { success = true, message = "Stock removed successfully" });
    }

    [HttpPost("adjust")]
    [Authorize(Roles = "Admin,Manager")] // Only Admin and Manager can adjust stock
    public async Task<IActionResult> AdjustStock([FromBody] AdjustStockCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new { success = true, message = "Stock adjusted successfully" });
    }
}
