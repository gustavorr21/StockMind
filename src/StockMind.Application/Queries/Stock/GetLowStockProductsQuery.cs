using StockMind.Application.Common;
using StockMind.Application.DTOs;

namespace StockMind.Application.Queries.Stock;

public sealed record GetLowStockProductsQuery : IQuery<Result<IEnumerable<StockDto>>>;
