using Microsoft.Extensions.Logging;
using StockMind.Application.Interfaces;

namespace StockMind.Infrastructure.Messaging;

public class DummyEventPublisher : IEventPublisher
{
    private readonly ILogger<DummyEventPublisher> _logger;

    public DummyEventPublisher(ILogger<DummyEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class
    {
        _logger.LogInformation("Event {EventType} would be published (dummy implementation)", typeof(TEvent).Name);
        return Task.CompletedTask;
    }
}
