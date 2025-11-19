using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Orders.Application.Abstractions.Data;
using Modules.Orders.Domain.Orders;
using Modules.Orders.Infrastructure.Persistence;
using Modules.Orders.Infrastructure.Persistence.Repositories;

namespace Modules.Orders.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrdersInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Database connection string is not configured");

        services.AddDbContext<OrdersDbContext>(options =>
            options
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IOrdersUnitOfWork>(sp =>
            sp.GetRequiredService<OrdersDbContext>());

        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }
}
