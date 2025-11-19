namespace Modules.Orders.Application.Orders;

public sealed record OrderDto(
    Guid Id,
    Guid CustomerId,
    string Status,
    decimal TotalAmount,
    string Currency,
    AddressDto ShippingAddress,
    List<OrderItemDto> Items,
    DateTime CreatedAt,
    DateTime? ConfirmedAt,
    DateTime? ShippedAt,
    DateTime? DeliveredAt,
    DateTime? CancelledAt);

public sealed record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal,
    string Currency);

public sealed record AddressDto(
    string Street,
    string City,
    string State,
    string Country,
    string PostalCode);
