using StockMind.Application.Common;
using StockMind.Domain.Enums;

namespace StockMind.Application.Commands.Stock;

public sealed record StockExitCommand(
    Guid ProductId,
    Guid WarehouseId,
    decimal Quantity,
    MovementOrigin Origin,
    string? Observation
) : ICommand<Result<Guid>>;
