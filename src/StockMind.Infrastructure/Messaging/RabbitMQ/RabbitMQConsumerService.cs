using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StockMind.Application.Interfaces;
using StockMind.Domain.Events;
using System.Text;
using System.Text.Json;

namespace StockMind.Infrastructure.Messaging.RabbitMQ;

public class RabbitMQConsumerService : BackgroundService
{
    private readonly ILogger<RabbitMQConsumerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _hostname;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMQConsumerService(
        ILogger<RabbitMQConsumerService> logger,
        IServiceProvider serviceProvider,
        string hostname)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _hostname = hostname;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("?? Starting RabbitMQ Consumer Service...");

            var factory = new ConnectionFactory() { HostName = _hostname };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Subscribe to LowStockDetectedEvent
            var queueName = nameof(LowStockDetectedEvent);
            _channel.QueueDeclare(
                queue: queueName, 
                durable: true, 
                exclusive: false, 
                autoDelete: false, 
                arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    
                    _logger.LogInformation("?? Received message from queue {QueueName}", queueName);
                    
                    var @event = JsonSerializer.Deserialize<LowStockDetectedEvent>(message);
                    
                    if (@event != null)
                    {
                        await ProcessLowStockEvent(@event);
                        _logger.LogInformation("? Message processed successfully");
                    }
                    
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "? Error processing message from RabbitMQ");
                    _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
            
            _logger.LogInformation("? RabbitMQ Consumer Service started successfully. Listening to queue: {QueueName}", queueName);

            // Keep running until cancellation is requested
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error starting RabbitMQ Consumer Service");
            throw;
        }
    }

    private async Task ProcessLowStockEvent(LowStockDetectedEvent @event)
    {
        try
        {
            _logger.LogInformation(
                "?? Processing low stock alert: Product {ProductName}, Current: {Current}, Minimum: {Minimum}",
                @event.ProductName,
                @event.CurrentQuantity,
                @event.MinimumQuantity);

            // Get SignalR Notifier from DI
            using var scope = _serviceProvider.CreateScope();
            var signalRNotifier = scope.ServiceProvider.GetService<ISignalRNotifier>();

            if (signalRNotifier != null)
            {
                // Send notification to all connected clients via SignalR
                await signalRNotifier.NotifyLowStockAsync(new
                {
                    ProductId = @event.ProductId,
                    ProductName = @event.ProductName,
                    ProductSku = @event.ProductSku,
                    WarehouseName = @event.WarehouseName,
                    CurrentQuantity = @event.CurrentQuantity,
                    MinimumQuantity = @event.MinimumQuantity,
                    Timestamp = DateTime.UtcNow
                });

                _logger.LogInformation("?? SignalR notification sent to all clients for product {ProductName}", @event.ProductName);
            }
            else
            {
                _logger.LogWarning("?? SignalR Notifier not configured. Notification skipped.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error processing low stock event");
            throw;
        }
    }

    public override void Dispose()
    {
        _logger.LogInformation("?? Stopping RabbitMQ Consumer Service...");
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
