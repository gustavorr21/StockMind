using StockMind.Application.Common;
using StockMind.Application.DTOs;

namespace StockMind.Application.Queries.Stock;

public sealed record GetStockByProductIdQuery(Guid ProductId) : IQuery<Result<StockDto>>;
