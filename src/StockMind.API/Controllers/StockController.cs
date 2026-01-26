using MediatR;
using Microsoft.AspNetCore.Mvc;
using StockMind.Application.Commands.Stock;
using StockMind.Application.Queries.Stock;

namespace StockMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockController : BaseController
{
    private readonly IMediator _mediator;

    public StockController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("product/{productId:guid}")]
    public async Task<IActionResult> GetByProductId(Guid productId)
    {
        var query = new GetStockByProductIdQuery(productId);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Data);
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock()
    {
        var query = new GetLowStockProductsQuery();
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Data);
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddStock([FromBody] AddStockCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new { success = true, message = "Stock added successfully" });
    }

    [HttpPost("remove")]
    public async Task<IActionResult> RemoveStock([FromBody] RemoveStockCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new { success = true, message = "Stock removed successfully" });
    }

    [HttpPost("adjust")]
    public async Task<IActionResult> AdjustStock([FromBody] AdjustStockCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new { success = true, message = "Stock adjusted successfully" });
    }
}
