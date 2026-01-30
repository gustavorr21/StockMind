using StockMind.Domain.Entities;

namespace StockMind.Domain.Repositories;

public interface IWarehouseRepository : IRepository<Warehouse>
{
    Task<Warehouse?> GetMainWarehouseAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Warehouse>> GetActiveWarehousesAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
