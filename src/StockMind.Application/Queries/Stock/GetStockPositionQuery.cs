using StockMind.Application.Common;
using StockMind.Application.DTOs.Stock;

namespace StockMind.Application.Queries.Stock;

public sealed record GetStockPositionQuery(
    Guid? ProductId,
    Guid? WarehouseId
) : IQuery<Result<List<StockPositionDto>>>;
