namespace Modules.Orders.Domain.Orders;

public enum OrderStatus
{
    Draft = 1,
    Pending = 2,
    Confirmed = 3,
    Processing = 4,
    Shipped = 5,
    Delivered = 6,
    Cancelled = 7,
    Refunded = 8
}
