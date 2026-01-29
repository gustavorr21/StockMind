using StockMind.Domain.Entities;
using StockMind.Domain.Enums;

namespace StockMind.Domain.Repositories;

public interface IStockMovementRepository : IRepository<StockMovement>
{
    Task<IEnumerable<StockMovement>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<StockMovement>> GetByWarehouseIdAsync(Guid warehouseId, CancellationToken cancellationToken = default);
    Task<IEnumerable<StockMovement>> GetByProductAndWarehouseAsync(Guid productId, Guid warehouseId, CancellationToken cancellationToken = default);
    Task<IEnumerable<StockMovement>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<StockMovement>> GetByTypeAsync(MovementType type, CancellationToken cancellationToken = default);
    Task<IEnumerable<StockMovement>> GetByOriginAsync(MovementOrigin origin, CancellationToken cancellationToken = default);
    Task<decimal> GetCurrentBalanceAsync(Guid productId, Guid warehouseId, CancellationToken cancellationToken = default);
    
    // Novo método para consulta paginada com filtros
    Task<(IEnumerable<StockMovement> Items, int TotalCount)> GetMovementsAsync(
        Guid? productId,
        Guid? warehouseId,
        DateTime? startDate,
        DateTime? endDate,
        string? type,
        string? origin,
        int skip,
        int take,
        string? sortBy,
        bool isDescending,
        CancellationToken cancellationToken = default);
}
