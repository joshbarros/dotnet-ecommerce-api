namespace Modules.Orders.Domain.Orders;

public sealed record OrderId(Guid Value)
{
    public static OrderId New() => new(Guid.NewGuid());

    public static OrderId Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Order ID cannot be empty", nameof(value));
        }

        return new OrderId(value);
    }

    public static implicit operator Guid(OrderId orderId) => orderId.Value;

    public override string ToString() => Value.ToString();
}
