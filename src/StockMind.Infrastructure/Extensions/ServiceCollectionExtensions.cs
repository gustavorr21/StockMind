using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using StockMind.Domain.Repositories;
using StockMind.Infrastructure.Caching;
using StockMind.Infrastructure.Messaging;
using StockMind.Infrastructure.Messaging.RabbitMQ;
using StockMind.Infrastructure.Persistence;
using StockMind.Infrastructure.Persistence.Repositories;

namespace StockMind.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IStockItemRepository, StockItemRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IStockMovementRepository, StockMovementRepository>();

        // Redis Cache (optional - will fail gracefully if not available)
        try
        {
            var redisConfiguration = configuration["Redis:Configuration"];
            if (!string.IsNullOrEmpty(redisConfiguration))
            {
                services.AddSingleton<IConnectionMultiplexer>(sp =>
                    ConnectionMultiplexer.Connect(redisConfiguration));
                services.AddScoped<ICacheService, RedisCacheService>();
            }
        }
        catch
        {
            // Redis not configured, skip
        }

        // RabbitMQ Event Bus (optional - will fail gracefully if not available)
        try
        {
            var rabbitMqHost = configuration["RabbitMQ:HostName"];
            if (!string.IsNullOrEmpty(rabbitMqHost))
            {
                services.AddSingleton<IEventBus>(sp => new RabbitMQEventBus(rabbitMqHost));
            }
        }
        catch
        {
            // RabbitMQ not configured, skip
        }

        return services;
    }
}
