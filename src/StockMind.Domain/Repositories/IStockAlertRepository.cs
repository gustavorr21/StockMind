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
}
