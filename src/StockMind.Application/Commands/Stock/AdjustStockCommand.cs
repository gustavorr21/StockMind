using StockMind.Application.Common;

namespace StockMind.Application.Commands.Stock;

public sealed record AdjustStockCommand(
    Guid ProductId,
    int NewQuantity,
    string? Reference,
    string? Notes
) : ICommand<Result<bool>>;
