using Microsoft.EntityFrameworkCore;
using StockMind.Domain.Entities;
using StockMind.Domain.Enums;
using StockMind.Domain.Repositories;

namespace StockMind.Infrastructure.Persistence.Repositories;

public class StockAlertRepository : IStockAlertRepository
{
    private readonly ApplicationDbContext _context;

    public StockAlertRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StockAlert?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.StockAlerts
            .Include(a => a.Product)
            .Include(a => a.Warehouse)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<StockAlert>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.StockAlerts
            .Include(a => a.Product)
            .Include(a => a.Warehouse)
            .OrderByDescending(a => a.FirstDetectedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<StockAlert?> GetActiveAlertAsync(
        Guid productId,
        Guid warehouseId,
        CancellationToken cancellationToken = default)
    {
        return await _context.StockAlerts
            .Where(a => a.ProductId == productId 
                     && a.WarehouseId == warehouseId
                     && (a.Status == AlertStatus.Active || a.Status == AlertStatus.Acknowledged))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<StockAlert>> GetActiveAlertsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.StockAlerts
            .Include(a => a.Product)
            .Include(a => a.Warehouse)
            .Where(a => a.Status == AlertStatus.Active)
            .OrderByDescending(a => a.FirstDetectedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StockAlert>> GetAlertsByStatusAsync(
        AlertStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _context.StockAlerts
            .Include(a => a.Product)
            .Include(a => a.Warehouse)
            .Where(a => a.Status == status)
            .OrderByDescending(a => a.FirstDetectedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetActiveAlertsCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.StockAlerts
            .CountAsync(a => a.Status == AlertStatus.Active, cancellationToken);
    }

    public async Task<StockAlert> AddAsync(StockAlert entity, CancellationToken cancellationToken = default)
    {
        await _context.StockAlerts.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(StockAlert entity, CancellationToken cancellationToken = default)
    {
        _context.StockAlerts.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            _context.StockAlerts.Remove(entity);
        }
    }
}
