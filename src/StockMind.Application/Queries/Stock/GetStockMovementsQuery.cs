using StockMind.Application.Common;
using StockMind.Application.DTOs.Stock;

namespace StockMind.Application.Queries.Stock;

public sealed record GetStockMovementsQuery(MovementSearchParams Params) : IQuery<Result<MovementListResponse>>;
