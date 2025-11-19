namespace Modules.Catalog.Domain.Categories;

/// <summary>
/// Strongly-typed Category identifier
/// </summary>
public sealed record CategoryId(Guid Value)
{
    public static CategoryId New() => new(Guid.NewGuid());
    public static CategoryId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(CategoryId id) => id.Value;
}
