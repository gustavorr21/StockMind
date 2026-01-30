using Microsoft.Extensions.Logging;
using StockMind.Application.Interfaces;

namespace StockMind.Infrastructure.Messaging.RabbitMQ;

public class RabbitMQEventPublisher : IEventPublisher, IDisposable
{
    private readonly RabbitMQEventBus _eventBus;
    private readonly ILogger<RabbitMQEventPublisher> _logger;

    public RabbitMQEventPublisher(string hostname, ILogger<RabbitMQEventPublisher> logger)
    {
        _eventBus = new RabbitMQEventBus(hostname);
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) 
        where TEvent : class
    {
        try
        {
            _logger.LogInformation(
                "?? Publishing event {EventType} to RabbitMQ queue", 
                typeof(TEvent).Name);
            
            await _eventBus.PublishAsync(@event, cancellationToken);
            
            _logger.LogInformation(
                "? Event {EventType} published successfully to RabbitMQ", 
                typeof(TEvent).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "? Error publishing event {EventType} to RabbitMQ", 
                typeof(TEvent).Name);
            throw;
        }
    }

    public void Dispose()
    {
        _eventBus?.Dispose();
    }
}
