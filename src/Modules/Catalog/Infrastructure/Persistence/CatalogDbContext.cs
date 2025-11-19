using Common.Domain;
using Microsoft.EntityFrameworkCore;
using Modules.Catalog.Domain.Categories;
using Modules.Catalog.Domain.Products;

namespace Modules.Catalog.Infrastructure.Persistence;

/// <summary>
/// Database context for Catalog module
/// </summary>
public sealed class CatalogDbContext : DbContext, IUnitOfWork
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Use the catalog schema
        modelBuilder.HasDefaultSchema("catalog");

        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch domain events before saving
        var domainEvents = ChangeTracker
            .Entries<AggregateRoot<ProductId>>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .SelectMany(e =>
            {
                var events = e.DomainEvents.ToList();
                e.ClearDomainEvents();
                return events;
            })
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        // TODO: Publish domain events via MediatR after successful save
        // This will be done through a domain event dispatcher

        return result;
    }
}
