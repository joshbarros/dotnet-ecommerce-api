using Common.Domain;

namespace Modules.Customers.Domain.Customers.Events;

public sealed record CustomerSuspendedEvent(
    CustomerId CustomerId,
    string Reason) : IDomainEvent;
