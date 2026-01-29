using Microsoft.EntityFrameworkCore;
using StockMind.Domain.Entities;
using StockMind.Domain.Repositories;

namespace StockMind.Infrastructure.Persistence.Repositories;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly ApplicationDbContext _context;

    public WarehouseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Warehouse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Warehouses
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Warehouse?> GetMainWarehouseAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Warehouses
            .FirstOrDefaultAsync(w => w.IsMain && w.IsActive, cancellationToken);
    }

    public async Task<IEnumerable<Warehouse>> GetActiveWarehousesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Warehouses
            .Where(w => w.IsActive)
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Warehouses
            .AnyAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<Warehouse> AddAsync(Warehouse entity, CancellationToken cancellationToken = default)
    {
        await _context.Warehouses.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task<Warehouse> UpdateAsync(Warehouse entity, CancellationToken cancellationToken = default)
    {
        _context.Warehouses.Update(entity);
        return Task.FromResult(entity);
    }

    public Task DeleteAsync(Warehouse entity, CancellationToken cancellationToken = default)
    {
        _context.Warehouses.Remove(entity);
        return Task.CompletedTask;
    }
}
