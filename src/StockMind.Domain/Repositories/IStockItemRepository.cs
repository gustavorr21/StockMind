using StockMind.Domain.Entities;

namespace StockMind.Domain.Repositories;

public interface IStockItemRepository : IRepository<StockItem>
{
    Task<StockItem?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<StockItem>> GetLowStockItemsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<StockItem>> GetByLocationAsync(string location, CancellationToken cancellationToken = default);
    Task<bool> ExistsForProductAsync(Guid productId, CancellationToken cancellationToken = default);
}
