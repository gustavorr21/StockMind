using StockMind.Application.Common;
using StockMind.Application.DTOs;

namespace StockMind.Application.Queries.Products;

public sealed record GetProductByIdQuery(Guid Id) : IQuery<Result<ProductDto>>;
