using Microsoft.EntityFrameworkCore;
using StockMind.Domain.Entities;
using StockMind.Domain.Enums;
using StockMind.Domain.Repositories;

namespace StockMind.Infrastructure.Persistence.Repositories;

public class SupplierRepository : Repository<Supplier>, ISupplierRepository
{
    public SupplierRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Supplier?> GetByDocumentAsync(string document, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.Document == document, cancellationToken);
    }

    public async Task<IEnumerable<Supplier>> GetByStatusAsync(SupplierStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.Status == status)
            .OrderBy(s => s.CompanyName)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsDocumentAsync(string document, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(s => s.Document == document, cancellationToken);
    }
}
