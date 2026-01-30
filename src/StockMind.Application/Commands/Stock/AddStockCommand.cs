using StockMind.Application.Common;

namespace StockMind.Application.Commands.Stock;

public sealed record AddStockCommand(
    Guid ProductId,
    int Quantity,
    string? Reference,
    string? Notes
) : ICommand<Result<bool>>;
