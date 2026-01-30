using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using StockMind.Application.Behaviors;
using System.Reflection;

namespace StockMind.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Add MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // Add FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
