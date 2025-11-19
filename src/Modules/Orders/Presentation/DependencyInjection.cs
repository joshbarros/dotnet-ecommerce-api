using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Modules.Orders.Application;
using Modules.Orders.Infrastructure;
using Modules.Orders.Presentation.Endpoints;

namespace Modules.Orders.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddOrdersModule(
        this IServiceCollection services,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        // Register MediatR
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(AssemblyReference.Assembly);
            config.RegisterServicesFromAssembly(Application.AssemblyReference.Assembly);
        });

        // Register FluentValidation
        services.AddValidatorsFromAssembly(
            Application.AssemblyReference.Assembly,
            includeInternalTypes: true);

        // Register Infrastructure
        services.AddOrdersInfrastructure(configuration);

        return services;
    }

    public static IEndpointRouteBuilder MapOrdersEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapOrderEndpoints();

        return app;
    }
}
