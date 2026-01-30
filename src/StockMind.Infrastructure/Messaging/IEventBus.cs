namespace StockMind.Infrastructure.Messaging;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class;
    Task SubscribeAsync<TEvent>(Func<TEvent, Task> handler) where TEvent : class;
}
