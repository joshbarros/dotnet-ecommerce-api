using Common.Domain;
using Modules.Customers.Domain.Customers;

namespace Modules.Orders.Domain.Orders.Events;

public sealed record OrderCreatedEvent(
    OrderId OrderId,
    CustomerId CustomerId) : IDomainEvent;
