using StockMind.Application.Common;

namespace StockMind.Application.Commands.Categories;

public sealed record CreateCategoryCommand(
    string Name,
    string Description
) : ICommand<Result<Guid>>;
