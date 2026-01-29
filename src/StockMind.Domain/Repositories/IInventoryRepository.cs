using StockMind.Domain.Entities;

namespace StockMind.Domain.Repositories;

public interface IInventoryRepository : IRepository<Inventory>
{
    Task<IEnumerable<Inventory>> GetByWarehouseIdAsync(Guid warehouseId, CancellationToken cancellationToken = default);
    Task<Inventory?> GetByInventoryNumberAsync(string inventoryNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Inventory>> GetInProgressInventoriesAsync(CancellationToken cancellationToken = default);
    Task<bool> InventoryNumberExistsAsync(string inventoryNumber, CancellationToken cancellationToken = default);
}
