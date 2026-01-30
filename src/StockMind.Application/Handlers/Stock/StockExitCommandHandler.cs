using MediatR;
using Microsoft.Extensions.Logging;
using StockMind.Application.Commands.Stock;
using StockMind.Application.Common;
using StockMind.Application.Interfaces;
using StockMind.Domain.Entities;
using StockMind.Domain.Enums;
using StockMind.Domain.Events;
using StockMind.Domain.Repositories;

namespace StockMind.Application.Handlers.Stock;

public class StockExitCommandHandler : ICommandHandler<StockExitCommand, Result<Guid>>
{
    private readonly IStockRepository _stockRepository;
    private readonly IStockMovementRepository _movementRepository;
    private readonly IProductRepository _productRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IStockAlertRepository _stockAlertRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;
    private readonly ILogger<StockExitCommandHandler> _logger;

    public StockExitCommandHandler(
        IStockRepository stockRepository,
        IStockMovementRepository movementRepository,
        IProductRepository productRepository,
        IWarehouseRepository warehouseRepository,
        IStockAlertRepository stockAlertRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMediator mediator,
        ILogger<StockExitCommandHandler> logger)
    {
        _stockRepository = stockRepository;
        _movementRepository = movementRepository;
        _productRepository = productRepository;
        _warehouseRepository = warehouseRepository;
        _stockAlertRepository = stockAlertRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mediator = mediator;
        _logger = logger;
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

            _logger.LogInformation(
                "Stock updated: ProductId={ProductId}, NewQuantity={NewQuantity}",
                request.ProductId,
                stock.CurrentQuantity);

            // Check for low stock alert
            await CheckAndCreateLowStockAlert(
                product, 
                warehouse, 
                stock.CurrentQuantity, 
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Stock exit completed successfully. MovementId={MovementId}", movement.Id);

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

    private async Task CheckAndCreateLowStockAlert(
        Product product,
        Warehouse warehouse,
        decimal currentQuantity,
        CancellationToken cancellationToken)
    {
        try
        {
            // Check if stock is below minimum threshold
            if (currentQuantity >= product.MinimumStock)
            {
                _logger.LogDebug(
                    "Stock level is adequate for product {ProductName} in warehouse {WarehouseName}. Current: {Current}, Minimum: {Minimum}",
                    product.Name,
                    warehouse.Name,
                    currentQuantity,
                    product.MinimumStock);
                return;
            }

            _logger.LogWarning(
                "Low stock detected for product {ProductName} ({ProductSku}) in warehouse {WarehouseName}. Current: {Current}, Minimum: {Minimum}",
                product.Name,
                product.Sku,
                warehouse.Name,
                currentQuantity,
                product.MinimumStock);

            // Check if an active alert already exists for this product/warehouse
            var existingAlert = await _stockAlertRepository.GetActiveAlertAsync(
                product.Id,
                warehouse.Id,
                cancellationToken);

            if (existingAlert != null)
            {
                // Update the quantity of the existing alert
                existingAlert.UpdateQuantity(currentQuantity);
                _logger.LogInformation(
                    "Updated existing alert for product {ProductName}. AlertId={AlertId}, NewQuantity={NewQuantity}",
                    product.Name,
                    existingAlert.Id,
                    currentQuantity);
            }
            else
            {
                // Create new alert
                var alert = StockAlert.Create(
                    product.Id,
                    product.Name,
                    product.Sku,
                    warehouse.Id,
                    warehouse.Name,
                    currentQuantity,
                    product.MinimumStock);

                await _stockAlertRepository.AddAsync(alert, cancellationToken);
                
                _logger.LogInformation(
                    "Created new stock alert for product {ProductName}. AlertId={AlertId}, ProductId={ProductId}",
                    product.Name,
                    alert.Id,
                    alert.ProductId);
            }

            // Publish event for asynchronous notifications (RabbitMQ + SignalR)
            var lowStockEvent = new LowStockDetectedEvent(
                product.Id,
                product.Name,
                product.Sku,
                warehouse.Id,
                warehouse.Name,
                currentQuantity,
                product.MinimumStock,
                currentQuantity);

            await _mediator.Publish(lowStockEvent, cancellationToken);
            
            _logger.LogInformation(
                "LowStockDetectedEvent published for product {ProductId}",
                product.Id);
        }
        catch (Exception ex)
        {
            // Log error but don't propagate to avoid breaking the main flow
            _logger.LogError(ex,
                "Error checking/creating low stock alert for product {ProductId}",
                product.Id);
        }
    }
}
