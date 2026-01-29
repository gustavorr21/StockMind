using Microsoft.EntityFrameworkCore;
using StockMind.Domain.Entities;
using StockMind.Domain.Enums;
using StockMind.Domain.Repositories;

namespace StockMind.Infrastructure.Persistence.Repositories;

public class StockMovementRepository : IStockMovementRepository
{
    private readonly ApplicationDbContext _context;

    public StockMovementRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StockMovement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.StockMovements
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .FirstOrDefaultAsync(sm => sm.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<StockMovement>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.StockMovements
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .OrderByDescending(sm => sm.MovementDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StockMovement>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.StockMovements
            .Include(sm => sm.Warehouse)
            .Where(sm => sm.ProductId == productId)
            .OrderByDescending(sm => sm.MovementDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StockMovement>> GetByWarehouseIdAsync(Guid warehouseId, CancellationToken cancellationToken = default)
    {
        return await _context.StockMovements
            .Include(sm => sm.Product)
            .Where(sm => sm.WarehouseId == warehouseId)
            .OrderByDescending(sm => sm.MovementDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StockMovement>> GetByProductAndWarehouseAsync(Guid productId, Guid warehouseId, CancellationToken cancellationToken = default)
    {
        return await _context.StockMovements
            .Where(sm => sm.ProductId == productId && sm.WarehouseId == warehouseId)
            .OrderByDescending(sm => sm.MovementDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StockMovement>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.StockMovements
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .Where(sm => sm.MovementDate >= startDate && sm.MovementDate <= endDate)
            .OrderByDescending(sm => sm.MovementDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StockMovement>> GetByTypeAsync(MovementType type, CancellationToken cancellationToken = default)
    {
        return await _context.StockMovements
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .Where(sm => sm.Type == type)
            .OrderByDescending(sm => sm.MovementDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StockMovement>> GetByOriginAsync(MovementOrigin origin, CancellationToken cancellationToken = default)
    {
        return await _context.StockMovements
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .Where(sm => sm.Origin == origin)
            .OrderByDescending(sm => sm.MovementDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetCurrentBalanceAsync(Guid productId, Guid warehouseId, CancellationToken cancellationToken = default)
    {
        var movements = await _context.StockMovements
            .Where(sm => sm.ProductId == productId && sm.WarehouseId == warehouseId)
            .OrderByDescending(sm => sm.MovementDate)
            .FirstOrDefaultAsync(cancellationToken);

        return movements?.NewBalance ?? 0;
    }

    public async Task<StockMovement> AddAsync(StockMovement entity, CancellationToken cancellationToken = default)
    {
        await _context.StockMovements.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(StockMovement entity, CancellationToken cancellationToken = default)
    {
        // StockMovement é APPEND-ONLY, mas mantemos o método para conformidade com a interface
        throw new InvalidOperationException("Stock movements cannot be updated. They are append-only.");
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // StockMovement é APPEND-ONLY, não deve ser deletado
        throw new InvalidOperationException("Stock movements cannot be deleted. They are append-only for audit purposes.");
    }
}
