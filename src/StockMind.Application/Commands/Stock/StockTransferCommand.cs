using StockMind.Application.Common;

namespace StockMind.Application.Commands.Stock;

public sealed record StockTransferCommand(
    Guid ProductId,
    Guid SourceWarehouseId,
    Guid DestinationWarehouseId,
    decimal Quantity,
    string? Observation
) : ICommand<Result<Guid>>;
