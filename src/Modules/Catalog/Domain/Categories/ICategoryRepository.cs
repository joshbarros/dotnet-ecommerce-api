using Common.Domain;

namespace Modules.Catalog.Domain.Categories;

/// <summary>
/// Repository interface for Category aggregate
/// </summary>
public interface ICategoryRepository : IRepository<Category, CategoryId>
{
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetChildrenAsync(CategoryId parentId, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
}
