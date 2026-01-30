using StockMind.Application.Common;
using StockMind.Application.DTOs;
using StockMind.Domain.Enums;

namespace StockMind.Application.Queries.Products;

public sealed record SearchProductsQuery(
    string? SearchTerm = null,
    Guid? CategoryId = null,
    Guid? SupplierId = null,
    ProductStatus? Status = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    int Page = 1,
    int PageSize = 20,
    string SortBy = "Name",
    string SortOrder = "asc"
) : IQuery<Result<PagedResult<ProductDto>>>;
