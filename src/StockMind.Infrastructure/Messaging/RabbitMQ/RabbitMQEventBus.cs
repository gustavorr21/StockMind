using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace StockMind.Infrastructure.Messaging.RabbitMQ;

public class RabbitMQEventBus : IEventBus
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMQEventBus(string hostname = "localhost")
    {
        var factory = new ConnectionFactory() { HostName = hostname };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class
    {
        var queueName = typeof(TEvent).Name;
        _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        var message = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
        await Task.CompletedTask;
    }

    public Task SubscribeAsync<TEvent>(Func<TEvent, Task> handler) where TEvent : class
    {
        // Implementation for consuming messages
        throw new NotImplementedException("Subscribe implementation will be added when needed");
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
