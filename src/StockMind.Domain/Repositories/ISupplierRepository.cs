using StockMind.Domain.Entities;
using StockMind.Domain.Enums;

namespace StockMind.Domain.Repositories;

public interface ISupplierRepository : IRepository<Supplier>
{
    Task<Supplier?> GetByDocumentAsync(string document, CancellationToken cancellationToken = default);
    Task<IEnumerable<Supplier>> GetByStatusAsync(SupplierStatus status, CancellationToken cancellationToken = default);
    Task<bool> ExistsDocumentAsync(string document, CancellationToken cancellationToken = default);
}
