using MediatR;
using Microsoft.Extensions.Logging;
using StockMind.Application.Interfaces;
using StockMind.Domain.Events;

namespace StockMind.Application.Handlers.Events;

public class LowStockDetectedEventHandler : INotificationHandler<LowStockDetectedEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<LowStockDetectedEventHandler> _logger;

    public LowStockDetectedEventHandler(
        IEventPublisher eventPublisher,
        ILogger<LowStockDetectedEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(LowStockDetectedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Low stock detected for product {ProductName} ({ProductSku}) in warehouse {WarehouseName}. " +
                "Current: {Current}, Minimum: {Minimum}",
                notification.ProductName,
                notification.ProductSku,
                notification.WarehouseName,
                notification.CurrentQuantity,
                notification.MinimumQuantity);

            // Publicar no RabbitMQ para processamento assíncrono
            await _eventPublisher.PublishAsync(notification, cancellationToken);

            _logger.LogInformation(
                "Low stock event published to RabbitMQ for product {ProductId}",
                notification.ProductId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error handling low stock event for product {ProductId}",
                notification.ProductId);
            
            // Não lançar exceção para não quebrar o fluxo principal
        }
    }
}
