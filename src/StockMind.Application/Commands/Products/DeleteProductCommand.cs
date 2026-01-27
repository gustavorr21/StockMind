using StockMind.Application.Common;

namespace StockMind.Application.Commands.Products;

public sealed record DeleteProductCommand(Guid Id) : ICommand<Result<bool>>;
