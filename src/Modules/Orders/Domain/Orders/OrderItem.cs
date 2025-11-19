using Common.Domain;
using Modules.Catalog.Domain.Products;
using Modules.Catalog.Domain.Common;

namespace Modules.Orders.Domain.Orders;

public sealed class OrderItem : Entity<OrderItemId>
{
    public ProductId ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = null!;
    public Money Subtotal { get; private set; } = null!;

    private OrderItem() { }

    internal static OrderItem Create(ProductId productId, string productName, int quantity, Money unitPrice)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        }

        var orderItem = new OrderItem
        {
            Id = new OrderItemId(Guid.NewGuid()),
            ProductId = productId,
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Subtotal = unitPrice.Multiply(quantity)
        };

        return orderItem;
    }

    internal void IncreaseQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        }

        Quantity += quantity;
        Subtotal = UnitPrice.Multiply(Quantity);
    }

    internal void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        }

        Quantity = quantity;
        Subtotal = UnitPrice.Multiply(Quantity);
    }
}

public sealed record OrderItemId(Guid Value)
{
    public static implicit operator Guid(OrderItemId id) => id.Value;
}
