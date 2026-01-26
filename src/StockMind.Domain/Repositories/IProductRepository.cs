using StockMind.Domain.Entities;

namespace StockMind.Domain.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetBySupplierIdAsync(Guid supplierId, CancellationToken cancellationToken = default);
    Task<bool> ExistsSkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default);
}
