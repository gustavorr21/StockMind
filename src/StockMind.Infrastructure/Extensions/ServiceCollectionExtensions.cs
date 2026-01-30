using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using StackExchange.Redis;
using StockMind.Application.Interfaces;
using StockMind.Domain.Repositories;
using StockMind.Infrastructure.Caching;
using StockMind.Infrastructure.Identity;
using StockMind.Infrastructure.Messaging;
using StockMind.Infrastructure.Messaging.RabbitMQ;
using StockMind.Infrastructure.Persistence;
using StockMind.Infrastructure.Persistence.Repositories;
using StockMind.Infrastructure.Services;

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

        // Identity
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<IdentityRole<Guid>>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>(
            TokenOptions.DefaultProvider);


        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // HttpContextAccessor (for getting current user in handlers)
        services.AddHttpContextAccessor();

        // Repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IStockItemRepository, StockItemRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IStockMovementRepository, StockMovementRepository>();
        
        // Novos repositórios do sistema de estoque profissional
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        services.AddScoped<IStockRepository, StockRepository>();
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IStockAlertRepository, StockAlertRepository>();

        // Authentication Services
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // File Storage Service
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        
        // Email Service
        services.AddScoped<IEmailService, EmailService>();
        
        // Event Publisher - RabbitMQ ou Dummy como fallback
        var rabbitMqHost = configuration["RabbitMQ:HostName"];
        if (!string.IsNullOrEmpty(rabbitMqHost))
        {
            try
            {
                // Registrar RabbitMQEventPublisher como IEventPublisher
                services.AddScoped<IEventPublisher>(sp => 
                    new RabbitMQEventPublisher(
                        rabbitMqHost, 
                        sp.GetRequiredService<ILogger<RabbitMQEventPublisher>>()));
                
                // Registrar RabbitMQ Consumer Service como Hosted Service
                services.AddHostedService(sp => 
                    new RabbitMQConsumerService(
                        sp.GetRequiredService<ILogger<RabbitMQConsumerService>>(),
                        sp,
                        rabbitMqHost));
                
                Console.WriteLine("? RabbitMQ Event Publisher and Consumer configured successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"?? Failed to configure RabbitMQ: {ex.Message}. Using DummyEventPublisher.");
                services.AddScoped<IEventPublisher, DummyEventPublisher>();
            }
        }
        else
        {
            Console.WriteLine("?? RabbitMQ:HostName not configured. Using DummyEventPublisher.");
            services.AddScoped<IEventPublisher, DummyEventPublisher>();
        }

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

        return services;
    }
}
