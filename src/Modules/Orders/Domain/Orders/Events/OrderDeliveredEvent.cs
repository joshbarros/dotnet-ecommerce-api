using Common.Domain;
using Modules.Customers.Domain.Customers;

namespace Modules.Orders.Domain.Orders.Events;

public sealed record OrderDeliveredEvent(
    OrderId OrderId,
    CustomerId CustomerId) : IDomainEvent;
