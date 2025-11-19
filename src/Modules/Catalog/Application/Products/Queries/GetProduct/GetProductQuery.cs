using Common.Application;
using Modules.Catalog.Application.Products.DTOs;
using Modules.Catalog.Domain.Products;

namespace Modules.Catalog.Application.Products.Queries.GetProduct;

/// <summary>
/// Query to get a product by ID
/// </summary>
public sealed record GetProductQuery(Guid ProductId) : IQuery<ProductDto?>;

/// <summary>
/// Handler for GetProductQuery
/// </summary>
internal sealed class GetProductQueryHandler : IQueryHandler<GetProductQuery, ProductDto?>
{
    private readonly IProductRepository _productRepository;

    public GetProductQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductDto?> Handle(
        GetProductQuery request,
        CancellationToken cancellationToken)
    {
        var productId = ProductId.From(request.ProductId);
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);

        if (product is null)
        {
            return null;
        }

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name.Value,
            Description = product.Description,
            Price = product.Price.Amount,
            Currency = product.Price.Currency,
            CategoryId = product.CategoryId,
            CategoryName = string.Empty, // TODO: Load from category
            Status = product.Status.ToString(),
            Images = product.Images.Select(img => new ProductImageDto
            {
                Id = img.Id,
                Url = img.Url,
                DisplayOrder = img.DisplayOrder
            }).ToList(),
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
