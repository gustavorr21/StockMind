using Microsoft.EntityFrameworkCore;
using StockMind.Domain.Entities;
using StockMind.Domain.Repositories;

namespace StockMind.Infrastructure.Persistence.Repositories;

public class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly ApplicationDbContext _context;

    public PurchaseOrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PurchaseOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PurchaseOrders
            .Include(po => po.Supplier)
            .Include(po => po.Items)
                .ThenInclude(i => i.Product)
            .Include(po => po.PurchaseEntries)
            .FirstOrDefaultAsync(po => po.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<PurchaseOrder>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PurchaseOrders
            .Include(po => po.Supplier)
            .Include(po => po.Items)
            .OrderByDescending(po => po.OrderDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PurchaseOrder>> GetBySupplierIdAsync(Guid supplierId, CancellationToken cancellationToken = default)
    {
        return await _context.PurchaseOrders
            .Include(po => po.Items)
            .Where(po => po.SupplierId == supplierId)
            .OrderByDescending(po => po.OrderDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<PurchaseOrder?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await _context.PurchaseOrders
            .Include(po => po.Supplier)
            .Include(po => po.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(po => po.OrderNumber == orderNumber, cancellationToken);
    }

    public async Task<IEnumerable<PurchaseOrder>> GetPendingOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PurchaseOrders
            .Include(po => po.Supplier)
            .Include(po => po.Items)
            .Where(po => po.Status == Domain.Enums.PurchaseOrderStatus.Pending ||
                        po.Status == Domain.Enums.PurchaseOrderStatus.Confirmed)
            .OrderBy(po => po.ExpectedDeliveryDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> OrderNumberExistsAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await _context.PurchaseOrders
            .AnyAsync(po => po.OrderNumber == orderNumber, cancellationToken);
    }

    public async Task<PurchaseOrder> AddAsync(PurchaseOrder entity, CancellationToken cancellationToken = default)
    {
        await _context.PurchaseOrders.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(PurchaseOrder entity, CancellationToken cancellationToken = default)
    {
        _context.PurchaseOrders.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            _context.PurchaseOrders.Remove(entity);
        }
    }
}
