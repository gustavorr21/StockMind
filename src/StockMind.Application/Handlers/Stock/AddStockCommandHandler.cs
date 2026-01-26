using StockMind.Application.Commands.Stock;
using StockMind.Application.Common;
using StockMind.Domain.Entities;
using StockMind.Domain.Enums;
using StockMind.Domain.Repositories;

namespace StockMind.Application.Handlers.Stock;

public class AddStockCommandHandler : ICommandHandler<AddStockCommand, Result<bool>>
{
    private readonly IStockItemRepository _stockItemRepository;
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddStockCommandHandler(
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

    public async Task<Result<bool>> Handle(AddStockCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get product
            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
            {
                return Result<bool>.Failure("Product not found");
            }

            // Get or create stock item
            var stockItem = await _stockItemRepository.GetByProductIdAsync(request.ProductId, cancellationToken);
            
            if (stockItem == null)
            {
                stockItem = StockItem.Create(request.ProductId, 0);
                await _stockItemRepository.AddAsync(stockItem, cancellationToken);
            }

            var previousQuantity = stockItem.Quantity;

            // Add stock
            stockItem.AddStock(request.Quantity, product.MinimumStock, product.Name);

            // Create stock movement record
            var movement = StockMovement.Create(
                request.ProductId,
                StockMovementType.Purchase,
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
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error adding stock: {ex.Message}");
        }
    }
}
