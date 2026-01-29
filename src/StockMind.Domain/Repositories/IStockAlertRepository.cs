using StockMind.Domain.Entities;
using StockMind.Domain.Enums;

namespace StockMind.Domain.Repositories;

public interface IStockAlertRepository : IRepository<StockAlert>
{
    Task<StockAlert?> GetActiveAlertAsync(
        Guid productId, 
        Guid warehouseId, 
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<StockAlert>> GetActiveAlertsAsync(
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<StockAlert>> GetAlertsByStatusAsync(
        AlertStatus status, 
        CancellationToken cancellationToken = default);
    
    Task<int> GetActiveAlertsCountAsync(
        CancellationToken cancellationToken = default);
    
    Task<List<StockAlert>> GetFilteredAlertsAsync(
        AlertStatus? status = null,
        Guid? productId = null,
        Guid? warehouseId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);
}
