using Microsoft.EntityFrameworkCore;
using Modules.Catalog.Domain.Categories;

namespace Modules.Catalog.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Category aggregate
/// </summary>
internal sealed class CategoryRepository : ICategoryRepository
{
    private readonly CatalogDbContext _context;

    public CategoryRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(
        CategoryId id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Category?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetChildrenAsync(
        CategoryId parentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Where(c => c.ParentId == parentId)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AnyAsync(c => c.Slug == slug, cancellationToken);
    }

    public async Task AddAsync(
        Category aggregate,
        CancellationToken cancellationToken = default)
    {
        await _context.Categories.AddAsync(aggregate, cancellationToken);
    }

    public void Update(Category aggregate)
    {
        _context.Categories.Update(aggregate);
    }

    public void Remove(Category aggregate)
    {
        _context.Categories.Remove(aggregate);
    }
}
