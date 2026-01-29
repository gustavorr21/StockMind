using StockMind.Application.Commands.Stock;
using StockMind.Application.Common;
using StockMind.Application.Interfaces;
using StockMind.Domain.Entities;
using StockMind.Domain.Repositories;

namespace StockMind.Application.Handlers.Stock;

public class StockExitCommandHandler : ICommandHandler<StockExitCommand, Result<Guid>>
{
    private readonly IStockRepository _stockRepository;
    private readonly IStockMovementRepository _movementRepository;
    private readonly IProductRepository _productRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public StockExitCommandHandler(
        IStockRepository stockRepository,
        IStockMovementRepository movementRepository,
        IProductRepository productRepository,
        IWarehouseRepository warehouseRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _stockRepository = stockRepository;
        _movementRepository = movementRepository;
        _productRepository = productRepository;
        _warehouseRepository = warehouseRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(StockExitCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get current user ID
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            {
                return Result<Guid>.Failure("User not authenticated");
            }

            // Validate product exists
            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
            {
                return Result<Guid>.Failure("Product not found");
            }

            // Validate warehouse exists
            var warehouse = await _warehouseRepository.GetByIdAsync(request.WarehouseId, cancellationToken);
            if (warehouse == null)
            {
                return Result<Guid>.Failure("Warehouse not found");
            }

            if (!warehouse.IsActive)
            {
                return Result<Guid>.Failure("Warehouse is not active");
            }

            // Get stock
            var stock = await _stockRepository.GetByProductAndWarehouseAsync(
                request.ProductId, request.WarehouseId, cancellationToken);

            if (stock == null)
            {
                return Result<Guid>.Failure("No stock available for this product in this warehouse");
            }

            // Check available quantity
            if (!stock.HasAvailableQuantity(request.Quantity))
            {
                return Result<Guid>.Failure($"Insufficient stock. Available: {stock.AvailableQuantity}, Requested: {request.Quantity}");
            }

            decimal previousBalance = stock.CurrentQuantity;

            // Create movement
            var movement = StockMovement.CreateExit(
                request.ProductId,
                request.WarehouseId,
                request.Origin,
                request.Quantity,
                previousBalance,
                userId.Value,
                request.Observation
            );

            await _movementRepository.AddAsync(movement, cancellationToken);

            // Update stock
            stock.RemoveQuantity(request.Quantity);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(movement.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Error removing stock: {ex.Message}");
        }
    }
}
