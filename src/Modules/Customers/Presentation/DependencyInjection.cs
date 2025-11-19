using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Modules.Customers.Application;
using Modules.Customers.Infrastructure;
using Modules.Customers.Presentation.Endpoints;

namespace Modules.Customers.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddCustomersModule(
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
        services.AddCustomersInfrastructure(configuration);

        return services;
    }

    public static IEndpointRouteBuilder MapCustomersEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapCustomerEndpoints();

        return app;
    }
}
