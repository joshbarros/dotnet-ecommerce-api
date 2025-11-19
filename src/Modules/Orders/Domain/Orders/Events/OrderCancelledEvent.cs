using Common.Domain;
using Modules.Customers.Domain.Customers;

namespace Modules.Orders.Domain.Orders.Events;

public sealed record OrderCancelledEvent(
    OrderId OrderId,
    CustomerId CustomerId,
    string Reason) : IDomainEvent;
