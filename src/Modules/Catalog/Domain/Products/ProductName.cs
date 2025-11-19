using Common.Domain;

namespace Modules.Catalog.Domain.Products;

/// <summary>
/// Value object representing a product name
/// </summary>
public sealed class ProductName : ValueObject
{
    public string Value { get; private init; }

    private ProductName(string value)
    {
        Value = value;
    }

    public static ProductName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Product name cannot be empty");
        }

        if (value.Length > 200)
        {
            throw new DomainException("Product name cannot exceed 200 characters");
        }

        return new ProductName(value.Trim());
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public static implicit operator string(ProductName name) => name.Value;
    public override string ToString() => Value;
}
