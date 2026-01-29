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
}
