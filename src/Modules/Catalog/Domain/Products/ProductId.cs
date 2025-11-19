namespace Modules.Catalog.Domain.Products;

/// <summary>
/// Strongly-typed Product identifier
/// </summary>
public sealed record ProductId(Guid Value)
{
    public static ProductId New() => new(Guid.NewGuid());
    public static ProductId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(ProductId id) => id.Value;
}
