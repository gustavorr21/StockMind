using StockMind.Application.Common;
using StockMind.Application.DTOs;
using StockMind.Application.Queries.Products;
using StockMind.Domain.Repositories;
using StockMind.Domain.Enums;

namespace StockMind.Application.Handlers.Products;

public class SearchProductsQueryHandler : IQueryHandler<SearchProductsQuery, Result<PagedResult<ProductDto>>>
{
    private readonly IProductRepository _productRepository;

    public SearchProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<PagedResult<ProductDto>>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get all products (in real scenario, this would be filtered at repository level)
            var allProducts = await _productRepository.GetAllAsync(cancellationToken);

            // Apply filters
            var query = allProducts.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLowerInvariant();
                query = query.Where(p => 
                    p.Name.ToLowerInvariant().Contains(searchLower) ||
                    p.Sku.ToLowerInvariant().Contains(searchLower) ||
                    (p.Description != null && p.Description.ToLowerInvariant().Contains(searchLower)) ||
                    (p.Barcode != null && p.Barcode.ToLowerInvariant().Contains(searchLower))
                );
            }

            if (request.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == request.CategoryId.Value);
            }

            if (request.SupplierId.HasValue)
            {
                query = query.Where(p => p.SupplierId == request.SupplierId.Value);
            }

            if (request.Status.HasValue)
            {
                query = query.Where(p => p.Status == request.Status.Value);
            }

            if (request.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price.Amount >= request.MinPrice.Value);
            }

            if (request.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price.Amount <= request.MaxPrice.Value);
            }

            // Get total count before paging
            var totalCount = query.Count();

            // Apply sorting
            query = request.SortBy.ToLowerInvariant() switch
            {
                "name" => request.SortOrder.ToLowerInvariant() == "desc" 
                    ? query.OrderByDescending(p => p.Name) 
                    : query.OrderBy(p => p.Name),
                "sku" => request.SortOrder.ToLowerInvariant() == "desc" 
                    ? query.OrderByDescending(p => p.Sku) 
                    : query.OrderBy(p => p.Sku),
                "price" => request.SortOrder.ToLowerInvariant() == "desc" 
                    ? query.OrderByDescending(p => p.Price.Amount) 
                    : query.OrderBy(p => p.Price.Amount),
                "createdat" => request.SortOrder.ToLowerInvariant() == "desc" 
                    ? query.OrderByDescending(p => p.CreatedAt) 
                    : query.OrderBy(p => p.CreatedAt),
                _ => query.OrderBy(p => p.Name)
            };

            // Apply paging
            var items = query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Sku = p.Sku,
                    Barcode = p.Barcode,
                    PriceAmount = p.Price.Amount,
                    PriceCurrency = p.Price.Currency,
                    CostPriceAmount = p.CostPrice.Amount,
                    CostPriceCurrency = p.CostPrice.Currency,
                    Status = p.Status,
                    CategoryId = p.CategoryId,
                    SupplierId = p.SupplierId,
                    MinimumStock = p.MinimumStock,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var pagedResult = new PagedResult<ProductDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = totalPages
            };

            return Result<PagedResult<ProductDto>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<ProductDto>>.Failure($"Error searching products: {ex.Message}");
        }
    }
}
