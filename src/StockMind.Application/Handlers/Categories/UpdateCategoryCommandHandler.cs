using StockMind.Application.Commands.Categories;
using StockMind.Application.Common;
using StockMind.Domain.Repositories;

namespace StockMind.Application.Handlers.Categories;

public class UpdateCategoryCommandHandler : ICommandHandler<UpdateCategoryCommand, Result<bool>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);

            if (category == null)
            {
                return Result<bool>.Failure("Category not found");
            }

            // Check if name already exists for another category
            var existingCategory = await _categoryRepository.GetByNameAsync(request.Name, cancellationToken);
            if (existingCategory != null && existingCategory.Id != request.Id)
            {
                return Result<bool>.Failure("Category name already exists");
            }

            // Update category
            category.UpdateName(request.Name);
            category.UpdateDescription(request.Description);

            await _categoryRepository.UpdateAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (ArgumentException ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error updating category: {ex.Message}");
        }
    }
}
