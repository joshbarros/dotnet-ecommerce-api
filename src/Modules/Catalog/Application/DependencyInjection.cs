using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Common.Application;

namespace Modules.Catalog.Application;

/// <summary>
/// Dependency injection configuration for Catalog Application layer
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddCatalogApplication(this IServiceCollection services)
    {
        // Register MediatR with all command/query handlers in this assembly
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

            // Register pipeline behaviors (order matters!)
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // Register all FluentValidation validators in this assembly
        services.AddValidatorsFromAssembly(
            typeof(DependencyInjection).Assembly,
            includeInternalTypes: true);

        return services;
    }
}
