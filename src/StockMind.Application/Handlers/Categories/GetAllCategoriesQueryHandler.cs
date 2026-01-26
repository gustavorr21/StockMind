using StockMind.Application.Common;
using StockMind.Application.DTOs;
using StockMind.Application.Queries.Categories;
using StockMind.Domain.Repositories;

namespace StockMind.Application.Handlers.Categories;

public class GetAllCategoriesQueryHandler : IQueryHandler<GetAllCategoriesQuery, Result<IEnumerable<CategoryDto>>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetAllCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<IEnumerable<CategoryDto>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var categories = await _categoryRepository.GetAllAsync(cancellationToken);

            var dtos = categories.Select(category => new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            });

            return Result<IEnumerable<CategoryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<CategoryDto>>.Failure($"Error retrieving categories: {ex.Message}");
        }
    }
}
