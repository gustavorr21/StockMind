using Microsoft.EntityFrameworkCore;
using StockMind.Domain.Entities;
using StockMind.Domain.Repositories;

namespace StockMind.Infrastructure.Persistence.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly ApplicationDbContext _context;

    public InventoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Inventory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Inventories
            .Include(i => i.Warehouse)
            .Include(i => i.Items)
                .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Inventory>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Inventories
            .Include(i => i.Warehouse)
            .OrderByDescending(i => i.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Inventory>> GetByWarehouseIdAsync(Guid warehouseId, CancellationToken cancellationToken = default)
    {
        return await _context.Inventories
            .Include(i => i.Items)
            .Where(i => i.WarehouseId == warehouseId)
            .OrderByDescending(i => i.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Inventory?> GetByInventoryNumberAsync(string inventoryNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Inventories
            .Include(i => i.Warehouse)
            .Include(i => i.Items)
                .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(i => i.InventoryNumber == inventoryNumber, cancellationToken);
    }

    public async Task<IEnumerable<Inventory>> GetInProgressInventoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Inventories
            .Include(i => i.Warehouse)
            .Include(i => i.Items)
            .Where(i => i.Status == Domain.Enums.InventoryStatus.InProgress)
            .OrderBy(i => i.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> InventoryNumberExistsAsync(string inventoryNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Inventories
            .AnyAsync(i => i.InventoryNumber == inventoryNumber, cancellationToken);
    }

    public async Task<Inventory> AddAsync(Inventory entity, CancellationToken cancellationToken = default)
    {
        await _context.Inventories.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(Inventory entity, CancellationToken cancellationToken = default)
    {
        _context.Inventories.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            _context.Inventories.Remove(entity);
        }
    }
}
