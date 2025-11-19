namespace Modules.Catalog.Presentation.Contracts.Requests;

/// <summary>
/// Request to create a new product
/// </summary>
public sealed record CreateProductRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
    public Guid CategoryId { get; init; }
}

/// <summary>
/// Request to update product information
/// </summary>
public sealed record UpdateProductRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

/// <summary>
/// Request to update product price
/// </summary>
public sealed record UpdateProductPriceRequest
{
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
}
