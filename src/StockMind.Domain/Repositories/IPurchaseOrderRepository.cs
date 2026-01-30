using StockMind.Domain.Entities;

namespace StockMind.Domain.Repositories;

public interface IPurchaseOrderRepository : IRepository<PurchaseOrder>
{
    Task<IEnumerable<PurchaseOrder>> GetBySupplierIdAsync(Guid supplierId, CancellationToken cancellationToken = default);
    Task<PurchaseOrder?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<PurchaseOrder>> GetPendingOrdersAsync(CancellationToken cancellationToken = default);
    Task<bool> OrderNumberExistsAsync(string orderNumber, CancellationToken cancellationToken = default);
}
