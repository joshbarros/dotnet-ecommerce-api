using Microsoft.EntityFrameworkCore;
using Modules.Catalog.Domain.Categories;
using Modules.Catalog.Domain.Products;

namespace Modules.Catalog.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Product aggregate
/// </summary>
internal sealed class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _context;

    public ProductRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(
        ProductId id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(
        CategoryId categoryId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Images)
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        ProductId id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task AddAsync(
        Product aggregate,
        CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(aggregate, cancellationToken);
    }

    public void Update(Product aggregate)
    {
        _context.Products.Update(aggregate);
    }

    public void Remove(Product aggregate)
    {
        _context.Products.Remove(aggregate);
    }
}
