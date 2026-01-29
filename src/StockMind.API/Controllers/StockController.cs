using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockMind.Application.Commands.Stock;
using StockMind.Application.Queries.Stock;

namespace StockMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockController : BaseController
{
    private readonly IMediator _mediator;

    public StockController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get stock position by product and/or warehouse
    /// </summary>
    [HttpGet("position")]
    [Authorize(Roles = "Admin,Manager,Operator,Viewer")]
    public async Task<IActionResult> GetPosition(
        [FromQuery] Guid? productId,
        [FromQuery] Guid? warehouseId)
    {
        var query = new GetStockPositionQuery(productId, warehouseId);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get stock movement history
    /// </summary>
    [HttpGet("movements")]
    [Authorize(Roles = "Admin,Manager,Operator,Viewer")]
    public async Task<IActionResult> GetMovements(
        [FromQuery] Guid? productId,
        [FromQuery] Guid? warehouseId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var query = new GetStockMovementHistoryQuery(productId, warehouseId, startDate, endDate);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Add stock (entry)
    /// </summary>
    [HttpPost("entry")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Entry([FromBody] StockEntryCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new { movementId = result.Data, message = "Stock added successfully" });
    }

    /// <summary>
    /// Remove stock (exit)
    /// </summary>
    [HttpPost("exit")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Exit([FromBody] StockExitCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new { movementId = result.Data, message = "Stock removed successfully" });
    }

    /// <summary>
    /// Transfer stock between warehouses
    /// </summary>
    [HttpPost("transfer")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Transfer([FromBody] StockTransferCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new { movementId = result.Data, message = "Stock transferred successfully" });
    }

    /// <summary>
    /// Get products with low stock
    /// </summary>
    [HttpGet("low-stock")]
    [Authorize(Roles = "Admin,Manager,Operator,Viewer")]
    public async Task<IActionResult> GetLowStock()
    {
        var query = new GetStockPositionQuery(null, null);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        var lowStock = result.Data?.Where(s => s.IsBelowMinimum).ToList();
        return Ok(lowStock);
    }

    /// <summary>
    /// Get products out of stock
    /// </summary>
    [HttpGet("out-of-stock")]
    [Authorize(Roles = "Admin,Manager,Operator,Viewer")]
    public async Task<IActionResult> GetOutOfStock()
    {
        var query = new GetStockPositionQuery(null, null);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        var outOfStock = result.Data?.Where(s => s.CurrentQuantity == 0).ToList();
        return Ok(outOfStock);
    }
}
