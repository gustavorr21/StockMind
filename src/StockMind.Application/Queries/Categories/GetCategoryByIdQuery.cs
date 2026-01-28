using StockMind.Application.Common;
using StockMind.Application.DTOs;

namespace StockMind.Application.Queries.Categories;

public sealed record GetCategoryByIdQuery(Guid Id) : IQuery<Result<CategoryDto>>;
