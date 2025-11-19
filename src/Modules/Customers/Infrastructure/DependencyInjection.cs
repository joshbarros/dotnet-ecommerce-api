using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Customers.Application.Abstractions.Data;
using Modules.Customers.Domain.Customers;
using Modules.Customers.Infrastructure.Persistence;
using Modules.Customers.Infrastructure.Persistence.Repositories;

namespace Modules.Customers.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCustomersInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Database connection string is not configured");

        services.AddDbContext<CustomersDbContext>(options =>
            options
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention());

        services.AddScoped<ICustomersUnitOfWork>(sp =>
            sp.GetRequiredService<CustomersDbContext>());

        services.AddScoped<ICustomerRepository, CustomerRepository>();

        return services;
    }
}
