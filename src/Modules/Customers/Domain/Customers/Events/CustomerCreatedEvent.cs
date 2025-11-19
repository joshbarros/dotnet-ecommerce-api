using Common.Domain;

namespace Modules.Customers.Domain.Customers.Events;

public sealed record CustomerCreatedEvent(
    CustomerId CustomerId,
    string CustomerName,
    string Email) : IDomainEvent;
