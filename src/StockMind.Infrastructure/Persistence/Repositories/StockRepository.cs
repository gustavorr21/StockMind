using Microsoft.EntityFrameworkCore;
using StockMind.Domain.Entities;
using StockMind.Domain.Repositories;

namespace StockMind.Infrastructure.Persistence.Repositories;

public class StockRepository : IStockRepository
{
    private readonly ApplicationDbContext _context;

    public StockRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Stock?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Stocks
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Stock>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Stocks
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .ToListAsync(cancellationToken);
    }

    public async Task<Stock?> GetByProductAndWarehouseAsync(Guid productId, Guid warehouseId, CancellationToken cancellationToken = default)
    {
        return await _context.Stocks
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == warehouseId, cancellationToken);
    }

    public async Task<IEnumerable<Stock>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.Stocks
            .Include(s => s.Warehouse)
            .Where(s => s.ProductId == productId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Stock>> GetByWarehouseIdAsync(Guid warehouseId, CancellationToken cancellationToken = default)
    {
        return await _context.Stocks
            .Include(s => s.Product)
            .Where(s => s.WarehouseId == warehouseId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Stock>> GetLowStockProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Stocks
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Where(s => s.Product != null && s.CurrentQuantity < s.Product.MinimumStock)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Stock>> GetOutOfStockProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Stocks
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Where(s => s.CurrentQuantity == 0)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalStockByProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.Stocks
            .Where(s => s.ProductId == productId)
            .SumAsync(s => s.CurrentQuantity, cancellationToken);
    }

    public async Task<Stock> AddAsync(Stock entity, CancellationToken cancellationToken = default)
    {
        await _context.Stocks.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(Stock entity, CancellationToken cancellationToken = default)
    {
        _context.Stocks.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            _context.Stocks.Remove(entity);
        }
    }

    public async Task<(IEnumerable<Stock> Items, int TotalCount)> GetStockItemsAsync(
        string? searchTerm,
        Guid? productId,
        Guid? categoryId,
        Guid? warehouseId,
        string? status,
        bool? onlyLowStock,
        bool? onlyCritical,
        int skip,
        int take,
        string? sortBy,
        bool isDescending,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Stocks
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .AsQueryable();

        // Filtro por termo de busca
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearch = searchTerm.ToLower();
            query = query.Where(s =>
                s.Product.Name.ToLower().Contains(lowerSearch) ||
                s.Product.Sku.ToLower().Contains(lowerSearch));
        }

        // Filtro por produto
        if (productId.HasValue)
        {
            query = query.Where(s => s.ProductId == productId.Value);
        }

        // Filtro por categoria
        if (categoryId.HasValue)
        {
            query = query.Where(s => s.Product.CategoryId == categoryId.Value);
        }

        // Filtro por depósito
        if (warehouseId.HasValue)
        {
            query = query.Where(s => s.WarehouseId == warehouseId.Value);
        }

        // Filtro por status
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Domain.Enums.ProductStatus>(status, out var productStatus))
        {
            query = query.Where(s => s.Product.Status == productStatus);
        }

        // Filtro por estoque baixo
        if (onlyLowStock == true)
        {
            query = query.Where(s => s.CurrentQuantity < s.Product.MinimumStock);
        }

        // Filtro por estoque crítico
        if (onlyCritical == true)
        {
            query = query.Where(s => s.CurrentQuantity == 0);
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

    private IQueryable<Stock> ApplySorting(IQueryable<Stock> query, string? sortBy, bool isDescending)
    {
        return sortBy?.ToLower() switch
        {
            "productname" => isDescending
                ? query.OrderByDescending(s => s.Product.Name)
                : query.OrderBy(s => s.Product.Name),

            "sku" => isDescending
                ? query.OrderByDescending(s => s.Product.Sku)
                : query.OrderBy(s => s.Product.Sku),

            "warehouse" => isDescending
                ? query.OrderByDescending(s => s.Warehouse.Name)
                : query.OrderBy(s => s.Warehouse.Name),

            "currentquantity" => isDescending
                ? query.OrderByDescending(s => s.CurrentQuantity)
                : query.OrderBy(s => s.CurrentQuantity),

            "availablequantity" => isDescending
                ? query.OrderByDescending(s => s.AvailableQuantity)
                : query.OrderBy(s => s.AvailableQuantity),

            "lastmovementdate" => isDescending
                ? query.OrderByDescending(s => s.LastMovementDate)
                : query.OrderBy(s => s.LastMovementDate),

            _ => query.OrderBy(s => s.Product.Name)
        };
    }
}
