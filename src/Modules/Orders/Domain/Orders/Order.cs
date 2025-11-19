using Common.Domain;
using Modules.Catalog.Domain.Common;
using Modules.Catalog.Domain.Products;
using Modules.Customers.Domain.Common;
using Modules.Customers.Domain.Customers;
using Modules.Orders.Domain.Orders.Events;

namespace Modules.Orders.Domain.Orders;

public sealed class Order : AggregateRoot<OrderId>
{
    private readonly List<OrderItem> _items = new();

    public CustomerId CustomerId { get; private set; } = null!;
    public OrderStatus Status { get; private set; }
    public Money TotalAmount { get; private set; } = null!;
    public Address ShippingAddress { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    private Order() { }

    public static Order Create(CustomerId customerId, Address shippingAddress)
    {
        var order = new Order
        {
            Id = OrderId.New(),
            CustomerId = customerId,
            Status = OrderStatus.Draft,
            ShippingAddress = shippingAddress,
            TotalAmount = Money.Usd(0),
            CreatedAt = DateTime.UtcNow
        };

        order.RaiseDomainEvent(new OrderCreatedEvent(order.Id, customerId));

        return order;
    }

    public Result AddItem(ProductId productId, string productName, int quantity, Money unitPrice)
    {
        if (Status != OrderStatus.Draft)
        {
            return Result.Failure(new Error(
                "Order.NotDraft",
                "Cannot modify order that is not in draft status"));
        }

        if (quantity <= 0)
        {
            return Result.Failure(new Error(
                "Order.InvalidQuantity",
                "Quantity must be positive"));
        }

        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            var orderItem = OrderItem.Create(productId, productName, quantity, unitPrice);
            _items.Add(orderItem);
        }

        RecalculateTotal();

        return Result.Success();
    }

    public Result RemoveItem(ProductId productId)
    {
        if (Status != OrderStatus.Draft)
        {
            return Result.Failure(new Error(
                "Order.NotDraft",
                "Cannot modify order that is not in draft status"));
        }

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
        {
            return Result.Failure(new Error(
                "Order.ItemNotFound",
                $"Product '{productId}' not found in order"));
        }

        _items.Remove(item);
        RecalculateTotal();

        return Result.Success();
    }

    public Result UpdateItemQuantity(ProductId productId, int quantity)
    {
        if (Status != OrderStatus.Draft)
        {
            return Result.Failure(new Error(
                "Order.NotDraft",
                "Cannot modify order that is not in draft status"));
        }

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
        {
            return Result.Failure(new Error(
                "Order.ItemNotFound",
                $"Product '{productId}' not found in order"));
        }

        item.UpdateQuantity(quantity);
        RecalculateTotal();

        return Result.Success();
    }

    public Result Submit()
    {
        if (Status != OrderStatus.Draft)
        {
            return Result.Failure(new Error(
                "Order.AlreadySubmitted",
                "Order has already been submitted"));
        }

        if (!_items.Any())
        {
            return Result.Failure(new Error(
                "Order.EmptyOrder",
                "Cannot submit an empty order"));
        }

        Status = OrderStatus.Pending;
        RaiseDomainEvent(new OrderSubmittedEvent(Id, CustomerId, TotalAmount));

        return Result.Success();
    }

    public Result Confirm()
    {
        if (Status != OrderStatus.Pending)
        {
            return Result.Failure(new Error(
                "Order.InvalidStatus",
                "Only pending orders can be confirmed"));
        }

        Status = OrderStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
        RaiseDomainEvent(new OrderConfirmedEvent(Id));

        return Result.Success();
    }

    public Result StartProcessing()
    {
        if (Status != OrderStatus.Confirmed)
        {
            return Result.Failure(new Error(
                "Order.InvalidStatus",
                "Only confirmed orders can be processed"));
        }

        Status = OrderStatus.Processing;

        return Result.Success();
    }

    public Result Ship()
    {
        if (Status != OrderStatus.Processing)
        {
            return Result.Failure(new Error(
                "Order.InvalidStatus",
                "Only processing orders can be shipped"));
        }

        Status = OrderStatus.Shipped;
        ShippedAt = DateTime.UtcNow;
        RaiseDomainEvent(new OrderShippedEvent(Id, CustomerId));

        return Result.Success();
    }

    public Result Deliver()
    {
        if (Status != OrderStatus.Shipped)
        {
            return Result.Failure(new Error(
                "Order.InvalidStatus",
                "Only shipped orders can be delivered"));
        }

        Status = OrderStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        RaiseDomainEvent(new OrderDeliveredEvent(Id, CustomerId));

        return Result.Success();
    }

    public Result Cancel(string reason)
    {
        if (Status is OrderStatus.Delivered or OrderStatus.Cancelled or OrderStatus.Refunded)
        {
            return Result.Failure(new Error(
                "Order.CannotCancel",
                $"Cannot cancel order in {Status} status"));
        }

        Status = OrderStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        RaiseDomainEvent(new OrderCancelledEvent(Id, CustomerId, reason));

        return Result.Success();
    }

    private void RecalculateTotal()
    {
        if (!_items.Any())
        {
            TotalAmount = Money.Usd(0);
            return;
        }

        var total = _items.Sum(i => i.Subtotal.Amount);
        TotalAmount = Money.Create(total, _items.First().UnitPrice.Currency);
    }
}
