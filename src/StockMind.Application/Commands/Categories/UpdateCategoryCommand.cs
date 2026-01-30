using StockMind.Application.Common;

namespace StockMind.Application.Commands.Categories;

public sealed record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string Description
) : ICommand<Result<bool>>;
