using StockMind.Application.Commands.Products;
using StockMind.Application.Common;
using StockMind.Domain.Repositories;

namespace StockMind.Application.Handlers.Products;

public class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand, Result<bool>>
{
    private readonly IProductRepository _productRepository;
    private readonly IStockItemRepository _stockItemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductCommandHandler(
        IProductRepository productRepository,
        IStockItemRepository stockItemRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _stockItemRepository = stockItemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id);
        
        if (product == null)
        {
            return Result<bool>.Failure("Product not found");
        }

        // Check if product has stock
        var stockItem = await _stockItemRepository.GetByProductIdAsync(request.Id);
        if (stockItem != null && stockItem.Quantity > 0)
        {
            return Result<bool>.Failure("Cannot delete product with existing stock. Remove stock first.");
        }

        // Soft delete
        product.Deactivate();
        
        await _productRepository.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
