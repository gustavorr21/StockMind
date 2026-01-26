using StockMind.Application.Common;
using StockMind.Application.DTOs;

namespace StockMind.Application.Queries.Products;

public sealed record GetAllProductsQuery : IQuery<Result<IEnumerable<ProductDto>>>;
