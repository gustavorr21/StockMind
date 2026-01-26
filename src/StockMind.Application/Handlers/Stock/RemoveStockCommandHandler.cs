using StockMind.Application.Commands.Stock;
using StockMind.Application.Common;
using StockMind.Domain.Entities;
using StockMind.Domain.Enums;
using StockMind.Domain.Repositories;

namespace StockMind.Application.Handlers.Stock;

public class RemoveStockCommandHandler : ICommandHandler<RemoveStockCommand, Result<bool>>
{
    private readonly IStockItemRepository _stockItemRepository;
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveStockCommandHandler(
        IStockItemRepository stockItemRepository,
        IStockMovementRepository stockMovementRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _stockItemRepository = stockItemRepository;
        _stockMovementRepository = stockMovementRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(RemoveStockCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get product
            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
            {
                return Result<bool>.Failure("Product not found");
            }

            // Get stock item
            var stockItem = await _stockItemRepository.GetByProductIdAsync(request.ProductId, cancellationToken);
            if (stockItem == null)
            {
                return Result<bool>.Failure("Stock item not found");
            }

            var previousQuantity = stockItem.Quantity;

            // Remove stock
            stockItem.RemoveStock(request.Quantity, product.MinimumStock, product.Name);

            // Create stock movement record
            var movement = StockMovement.Create(
                request.ProductId,
                StockMovementType.Sale,
                request.Quantity,
                previousQuantity,
                stockItem.Quantity,
                request.Reference,
                request.Notes
            );

            await _stockItemRepository.UpdateAsync(stockItem, cancellationToken);
            await _stockMovementRepository.AddAsync(movement, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (InvalidOperationException ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error removing stock: {ex.Message}");
        }
    }
}
