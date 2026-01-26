using Microsoft.EntityFrameworkCore;
using StockMind.Domain.Entities;
using StockMind.Domain.Repositories;

namespace StockMind.Infrastructure.Persistence.Repositories;

public class StockItemRepository : Repository<StockItem>, IStockItemRepository
{
    public StockItemRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<StockItem?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Product)
                .ThenInclude(p => p!.Category)
            .FirstOrDefaultAsync(s => s.ProductId == productId, cancellationToken);
    }

    public async Task<IEnumerable<StockItem>> GetLowStockItemsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Product)
                .ThenInclude(p => p!.Category)
            .Where(s => s.Quantity <= s.Product!.MinimumStock)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StockItem>> GetByLocationAsync(string location, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Product)
                .ThenInclude(p => p!.Category)
            .Where(s => s.Location == location)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsForProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(s => s.ProductId == productId, cancellationToken);
    }

    public override async Task<StockItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Product)
                .ThenInclude(p => p!.Category)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<StockItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Product)
                .ThenInclude(p => p!.Category)
            .ToListAsync(cancellationToken);
    }
}
