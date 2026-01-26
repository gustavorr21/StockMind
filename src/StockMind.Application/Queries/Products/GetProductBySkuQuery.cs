using StockMind.Application.Common;
using StockMind.Application.DTOs;

namespace StockMind.Application.Queries.Products;

public sealed record GetProductBySkuQuery(string Sku) : IQuery<Result<ProductDto>>;
