using StockMind.Domain.Entities;

namespace StockMind.Domain.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsNameAsync(string name, CancellationToken cancellationToken = default);
}
