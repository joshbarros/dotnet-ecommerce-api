using Common.Domain;
using Modules.Catalog.Domain.Categories;
using Modules.Catalog.Domain.Common;

namespace Modules.Catalog.Domain.Products;

/// <summary>
/// Product aggregate root
/// Represents a product in the catalog with its variants, images, and business rules
/// </summary>
public sealed class Product : AggregateRoot<ProductId>
{
    private readonly List<ProductImage> _images = new();

    private Product(
        ProductId id,
        ProductName name,
        string? description,
        Money price,
        CategoryId categoryId)
        : base(id)
    {
        Name = name;
        Description = description;
        Price = price;
        CategoryId = categoryId;
        Status = ProductStatus.Draft;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    private Product() : base()
    {
        // Required by EF Core
    }

    public ProductName Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Money Price { get; private set; } = null!;
    public CategoryId CategoryId { get; private set; } = null!;
    public ProductStatus Status { get; private set; }
    public DateTime CreatedAt { get; private init; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<ProductImage> Images => _images.AsReadOnly();

    /// <summary>
    /// Factory method to create a new product
    /// </summary>
    public static Product Create(
        ProductName name,
        string? description,
        Money price,
        CategoryId categoryId)
    {
        var productId = ProductId.New();

        var product = new Product(
            productId,
            name,
            description,
            price,
            categoryId);

        product.RaiseDomainEvent(new ProductCreatedEvent(
            productId,
            name.Value,
            price.Amount,
            price.Currency,
            categoryId.Value));

        return product;
    }

    /// <summary>
    /// Updates the product price
    /// </summary>
    public Result UpdatePrice(Money newPrice)
    {
        if (Status == ProductStatus.Discontinued)
        {
            return Result.Failure(new Error(
                "Product.Discontinued",
                "Cannot update price of discontinued product"));
        }

        var oldPrice = Price;
        Price = newPrice;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ProductPriceChangedEvent(
            Id,
            oldPrice.Amount,
            newPrice.Amount,
            newPrice.Currency));

        return Result.Success();
    }

    /// <summary>
    /// Updates product information
    /// </summary>
    public Result UpdateInformation(ProductName name, string? description)
    {
        if (Status == ProductStatus.Discontinued)
        {
            return Result.Failure(new Error(
                "Product.Discontinued",
                "Cannot update discontinued product"));
        }

        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ProductUpdatedEvent(Id, name.Value));

        return Result.Success();
    }

    /// <summary>
    /// Adds an image to the product
    /// </summary>
    public Result AddImage(string url, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return Result.Failure(new Error(
                "Product.InvalidImage",
                "Image URL cannot be empty"));
        }

        if (_images.Count >= 10)
        {
            return Result.Failure(new Error(
                "Product.TooManyImages",
                "Product cannot have more than 10 images"));
        }

        var image = new ProductImage(Id, url, displayOrder);
        _images.Add(image);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Activates the product for sale
    /// </summary>
    public Result Activate()
    {
        if (Status == ProductStatus.Discontinued)
        {
            return Result.Failure(new Error(
                "Product.Discontinued",
                "Cannot activate discontinued product"));
        }

        if (!_images.Any())
        {
            return Result.Failure(new Error(
                "Product.NoImages",
                "Product must have at least one image to be activated"));
        }

        Status = ProductStatus.Active;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ProductActivatedEvent(Id));

        return Result.Success();
    }

    /// <summary>
    /// Marks product as out of stock
    /// </summary>
    public Result MarkAsOutOfStock()
    {
        if (Status == ProductStatus.Discontinued)
        {
            return Result.Failure(new Error(
                "Product.Discontinued",
                "Discontinued product cannot be marked as out of stock"));
        }

        Status = ProductStatus.OutOfStock;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ProductOutOfStockEvent(Id));

        return Result.Success();
    }

    /// <summary>
    /// Discontinues the product permanently
    /// </summary>
    public Result Discontinue()
    {
        if (Status == ProductStatus.Discontinued)
        {
            return Result.Success(); // Already discontinued
        }

        Status = ProductStatus.Discontinued;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ProductDiscontinuedEvent(Id));

        return Result.Success();
    }
}

/// <summary>
/// Product image entity
/// </summary>
public sealed class ProductImage : Entity<Guid>
{
    internal ProductImage(ProductId productId, string url, int displayOrder)
        : base(Guid.NewGuid())
    {
        ProductId = productId;
        Url = url;
        DisplayOrder = displayOrder;
    }

    private ProductImage() : base()
    {
        // Required by EF Core
    }

    public ProductId ProductId { get; private init; } = null!;
    public string Url { get; private init; } = null!;
    public int DisplayOrder { get; private set; }
}
