using StockMind.Application.Commands.Categories;
using StockMind.Application.Common;
using StockMind.Domain.Repositories;

namespace StockMind.Application.Handlers.Categories;

public class DeleteCategoryCommandHandler : ICommandHandler<DeleteCategoryCommand, Result<bool>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);

            if (category == null)
            {
                return Result<bool>.Failure("Category not found");
            }

            // Check if category has products
            var products = await _productRepository.GetByCategoryIdAsync(request.Id, cancellationToken);
            if (products.Any())
            {
                return Result<bool>.Failure("Cannot delete category with existing products. Reassign products first.");
            }

            // Soft delete
            category.Deactivate();

            await _categoryRepository.UpdateAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error deleting category: {ex.Message}");
        }
    }
}
