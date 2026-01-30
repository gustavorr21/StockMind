using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockMind.Domain.Repositories;

namespace StockMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WarehouseController : BaseController
{
    private readonly IWarehouseRepository _warehouseRepository;

    public WarehouseController(IWarehouseRepository warehouseRepository)
    {
        _warehouseRepository = warehouseRepository;
    }

    /// <summary>
    /// Get all warehouses
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Operator,Viewer")]
    public async Task<IActionResult> GetAll()
    {
        var warehouses = await _warehouseRepository.GetAllAsync();
        return Ok(warehouses);
    }

    /// <summary>
    /// Get active warehouses only
    /// </summary>
    [HttpGet("active")]
    [Authorize(Roles = "Admin,Manager,Operator,Viewer")]
    public async Task<IActionResult> GetActive()
    {
        var warehouses = await _warehouseRepository.GetActiveWarehousesAsync();
        return Ok(warehouses);
    }

    /// <summary>
    /// Get warehouse by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Operator,Viewer")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(id);
        
        if (warehouse == null)
            return NotFound(new { error = "Warehouse not found" });

        return Ok(warehouse);
    }

    /// <summary>
    /// Get main warehouse
    /// </summary>
    [HttpGet("main")]
    [Authorize(Roles = "Admin,Manager,Operator,Viewer")]
    public async Task<IActionResult> GetMain()
    {
        var warehouse = await _warehouseRepository.GetMainWarehouseAsync();
        
        if (warehouse == null)
            return NotFound(new { error = "Main warehouse not found" });

        return Ok(warehouse);
    }
}
