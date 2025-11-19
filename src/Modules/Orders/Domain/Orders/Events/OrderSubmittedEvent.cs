using Common.Domain;
using Modules.Catalog.Domain.Common;
using Modules.Customers.Domain.Customers;

namespace Modules.Orders.Domain.Orders.Events;

public sealed record OrderSubmittedEvent(
    OrderId OrderId,
    CustomerId CustomerId,
    Money TotalAmount) : IDomainEvent;
