using StockMind.Domain.Entities;
using StockMind.Domain.Enums;

namespace StockMind.Domain.Repositories;

public interface IStockMovementRepository : IRepository<StockMovement>
{
    Task<IEnumerable<StockMovement>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<StockMovement>> GetByMovementTypeAsync(StockMovementType movementType, CancellationToken cancellationToken = default);
    Task<IEnumerable<StockMovement>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
