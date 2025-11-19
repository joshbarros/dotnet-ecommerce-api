using Common.Domain;

namespace Modules.Catalog.Domain.Products;

/// <summary>
/// Repository interface for Product aggregate
/// </summary>
public interface IProductRepository : IRepository<Product, ProductId>
{
    Task<IEnumerable<Product>> GetByCategoryAsync(
        Categories.CategoryId categoryId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(ProductId id, CancellationToken cancellationToken = default);
}
