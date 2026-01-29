using Microsoft.AspNetCore.Http;
using StockMind.Application.Commands.Stock;
using StockMind.Application.Common;
using StockMind.Domain.Entities;
using StockMind.Domain.Enums;
using StockMind.Domain.Repositories;
using System.Security.Claims;

namespace StockMind.Application.Handlers.Stock;

public class StockTransferCommandHandler : ICommandHandler<StockTransferCommand, Result<Guid>>
{
    private readonly IStockRepository _stockRepository;
    private readonly IStockMovementRepository _movementRepository;
    private readonly IProductRepository _productRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public StockTransferCommandHandler(
        IStockRepository stockRepository,
        IStockMovementRepository movementRepository,
        IProductRepository productRepository,
        IWarehouseRepository warehouseRepository,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor)
    {
        _stockRepository = stockRepository;
        _movementRepository = movementRepository;
        _productRepository = productRepository;
        _warehouseRepository = warehouseRepository;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<Guid>> Handle(StockTransferCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get current user ID
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Result<Guid>.Failure("User not authenticated");
            }

            // Validate product exists
            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
            {
                return Result<Guid>.Failure("Product not found");
            }

            // Validate source warehouse exists
            var sourceWarehouse = await _warehouseRepository.GetByIdAsync(request.SourceWarehouseId, cancellationToken);
            if (sourceWarehouse == null)
            {
                return Result<Guid>.Failure("Source warehouse not found");
            }

            if (!sourceWarehouse.IsActive)
            {
                return Result<Guid>.Failure("Source warehouse is not active");
            }

            // Validate destination warehouse exists
            var destinationWarehouse = await _warehouseRepository.GetByIdAsync(request.DestinationWarehouseId, cancellationToken);
            if (destinationWarehouse == null)
            {
                return Result<Guid>.Failure("Destination warehouse not found");
            }

            if (!destinationWarehouse.IsActive)
            {
                return Result<Guid>.Failure("Destination warehouse is not active");
            }

            if (request.SourceWarehouseId == request.DestinationWarehouseId)
            {
                return Result<Guid>.Failure("Source and destination warehouses cannot be the same");
            }

            // Get source stock
            var sourceStock = await _stockRepository.GetByProductAndWarehouseAsync(
                request.ProductId, request.SourceWarehouseId, cancellationToken);

            if (sourceStock == null)
            {
                return Result<Guid>.Failure("No stock available in source warehouse");
            }

            // Check available quantity in source
            if (!sourceStock.HasAvailableQuantity(request.Quantity))
            {
                return Result<Guid>.Failure($"Insufficient stock in source warehouse. Available: {sourceStock.AvailableQuantity}, Requested: {request.Quantity}");
            }

            // Get or create destination stock
            var destinationStock = await _stockRepository.GetByProductAndWarehouseAsync(
                request.ProductId, request.DestinationWarehouseId, cancellationToken);

            if (destinationStock == null)
            {
                destinationStock = Domain.Entities.Stock.Create(request.ProductId, request.DestinationWarehouseId);
                await _stockRepository.AddAsync(destinationStock, cancellationToken);
            }

            decimal sourcePreviousBalance = sourceStock.CurrentQuantity;
            decimal destinationPreviousBalance = destinationStock.CurrentQuantity;

            // Create exit movement from source
            var exitMovement = StockMovement.CreateTransfer(
                request.ProductId,
                request.SourceWarehouseId,
                request.DestinationWarehouseId,
                request.Quantity,
                sourcePreviousBalance,
                userId,
                request.Observation
            );

            await _movementRepository.AddAsync(exitMovement, cancellationToken);

            // Create entry movement to destination
            var entryMovement = StockMovement.CreateEntry(
                request.ProductId,
                request.DestinationWarehouseId,
                MovementOrigin.Transfer,
                request.Quantity,
                destinationPreviousBalance,
                userId,
                request.Observation
            );

            await _movementRepository.AddAsync(entryMovement, cancellationToken);

            // Update stocks
            sourceStock.RemoveQuantity(request.Quantity);
            destinationStock.AddQuantity(request.Quantity);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(exitMovement.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Error transferring stock: {ex.Message}");
        }
    }
}
