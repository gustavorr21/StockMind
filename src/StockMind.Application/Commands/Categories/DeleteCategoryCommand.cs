using StockMind.Application.Common;

namespace StockMind.Application.Commands.Categories;

public sealed record DeleteCategoryCommand(Guid Id) : ICommand<Result<bool>>;
