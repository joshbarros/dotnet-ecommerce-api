namespace Modules.Catalog.Domain.Products;

/// <summary>
/// Product availability status
/// </summary>
public enum ProductStatus
{
    Draft = 0,
    Active = 1,
    OutOfStock = 2,
    Discontinued = 3
}
