using StockMind.Application.Common;
using StockMind.Application.DTOs.Stock;

namespace StockMind.Application.Queries.Stock;

public sealed record GetStockMovementHistoryQuery(
    Guid? ProductId,
    Guid? WarehouseId,
    DateTime? StartDate,
    DateTime? EndDate
) : IQuery<Result<List<StockMovementDto>>>;
