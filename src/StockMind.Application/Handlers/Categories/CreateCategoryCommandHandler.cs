using StockMind.Application.Commands.Categories;
using StockMind.Application.Common;
using StockMind.Domain.Entities;
using StockMind.Domain.Repositories;

namespace StockMind.Application.Handlers.Categories;

public class CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand, Result<Guid>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if category name already exists
            if (await _categoryRepository.ExistsNameAsync(request.Name, cancellationToken))
            {
                return Result<Guid>.Failure($"Category with name '{request.Name}' already exists");
            }

            // Create category
            var category = Category.Create(request.Name, request.Description);

            await _categoryRepository.AddAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(category.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Error creating category: {ex.Message}");
        }
    }
}
