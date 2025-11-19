using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Catalog.Application;
using Modules.Catalog.Infrastructure;
using Modules.Catalog.Presentation.Endpoints;

namespace Modules.Catalog.Presentation;

/// <summary>
/// Catalog module registration
/// </summary>
public static class CatalogModule
{
    public static IServiceCollection AddCatalogModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add Application layer (CQRS, validation, etc.)
        services.AddCatalogApplication();

        // Add Infrastructure layer (database, repositories, etc.)
        services.AddCatalogInfrastructure(configuration);

        return services;
    }

    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapProductEndpoints();
        app.MapCategoryEndpoints();

        return app;
    }
}
