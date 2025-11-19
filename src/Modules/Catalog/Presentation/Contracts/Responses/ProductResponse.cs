namespace Modules.Catalog.Presentation.Contracts.Responses;

/// <summary>
/// Product response
/// </summary>
public sealed record ProductResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public string Currency { get; init; } = string.Empty;
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public List<ProductImageResponse> Images { get; init; } = new();
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// Product image response
/// </summary>
public sealed record ProductImageResponse
{
    public Guid Id { get; init; }
    public string Url { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
}
