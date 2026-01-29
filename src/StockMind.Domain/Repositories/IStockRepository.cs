using StockMind.Domain.Entities;

namespace StockMind.Domain.Repositories;

public interface IStockRepository : IRepository<Stock>
{
    Task<Stock?> GetByProductAndWarehouseAsync(Guid productId, Guid warehouseId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Stock>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Stock>> GetByWarehouseIdAsync(Guid warehouseId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Stock>> GetLowStockProductsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Stock>> GetOutOfStockProductsAsync(CancellationToken cancellationToken = default);
    Task<decimal> GetTotalStockByProductAsync(Guid productId, CancellationToken cancellationToken = default);
}
