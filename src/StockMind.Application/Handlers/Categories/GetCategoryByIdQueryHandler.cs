using StockMind.Application.Common;
using StockMind.Application.DTOs;
using StockMind.Application.Queries.Categories;
using StockMind.Domain.Repositories;

namespace StockMind.Application.Handlers.Categories;

public class GetCategoryByIdQueryHandler : IQueryHandler<GetCategoryByIdQuery, Result<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);

            if (category == null)
            {
                return Result<CategoryDto>.Failure("Category not found");
            }

            var dto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };

            return Result<CategoryDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<CategoryDto>.Failure($"Error retrieving category: {ex.Message}");
        }
    }
}
