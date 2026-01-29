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
        var endDateFinal = endDate.Date.AddDays(1).AddTicks(-1);

        return await _context.StockMovements
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .Where(sm => sm.MovementDate >= startDate
                      && sm.MovementDate <= endDateFinal)
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

    public async Task<(IEnumerable<StockMovement> Items, int TotalCount)> GetMovementsAsync(
        Guid? productId,
        Guid? warehouseId,
        DateTime? startDate,
        DateTime? endDate,
        string? type,
        string? origin,
        int skip,
        int take,
        string? sortBy,
        bool isDescending,
        CancellationToken cancellationToken = default)
    {
        var query = _context.StockMovements
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .AsQueryable();

        // Filtro por produto
        if (productId.HasValue)
        {
            query = query.Where(sm => sm.ProductId == productId.Value);
        }

        // Filtro por depósito
        if (warehouseId.HasValue)
        {
            query = query.Where(sm => sm.WarehouseId == warehouseId.Value);
        }

        // Filtro por período
        if (startDate.HasValue)
        {
            query = query.Where(sm => sm.MovementDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            var endDateExclusive = endDate.Value.Date.AddDays(1);

            query = query.Where(sm => sm.MovementDate <= endDateExclusive);
        }

        // Filtro por tipo
        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<MovementType>(type, out var movementType))
        {
            query = query.Where(sm => sm.Type == movementType);
        }

        // Filtro por origem
        if (!string.IsNullOrWhiteSpace(origin) && Enum.TryParse<MovementOrigin>(origin, out var movementOrigin))
        {
            query = query.Where(sm => sm.Origin == movementOrigin);
        }

        // Contagem total
        var totalCount = await query.CountAsync(cancellationToken);

        // Ordenação
        query = ApplySorting(query, sortBy, isDescending);

        // Paginação
        var items = await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    private IQueryable<StockMovement> ApplySorting(IQueryable<StockMovement> query, string? sortBy, bool isDescending)
    {
        return sortBy?.ToLower() switch
        {
            "productname" => isDescending
                ? query.OrderByDescending(sm => sm.Product.Name)
                : query.OrderBy(sm => sm.Product.Name),

            "warehouse" => isDescending
                ? query.OrderByDescending(sm => sm.Warehouse.Name)
                : query.OrderBy(sm => sm.Warehouse.Name),

            "type" => isDescending
                ? query.OrderByDescending(sm => sm.Type)
                : query.OrderBy(sm => sm.Type),

            "origin" => isDescending
                ? query.OrderByDescending(sm => sm.Origin)
                : query.OrderBy(sm => sm.Origin),

            "quantity" => isDescending
                ? query.OrderByDescending(sm => sm.Quantity)
                : query.OrderBy(sm => sm.Quantity),

            "movementdate" => isDescending
                ? query.OrderByDescending(sm => sm.MovementDate)
                : query.OrderBy(sm => sm.MovementDate),

            _ => query.OrderByDescending(sm => sm.MovementDate) // Default: mais recentes primeiro
        };
    }
}
