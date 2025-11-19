using Microsoft.EntityFrameworkCore;
using Modules.Customers.Application.Abstractions.Data;
using Modules.Customers.Domain.Customers;

namespace Modules.Customers.Infrastructure.Persistence;

public sealed class CustomersDbContext : DbContext, ICustomersUnitOfWork
{
    public CustomersDbContext(DbContextOptions<CustomersDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("customers");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomersDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
