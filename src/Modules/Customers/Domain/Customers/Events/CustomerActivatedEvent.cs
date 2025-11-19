using Common.Domain;

namespace Modules.Customers.Domain.Customers.Events;

public sealed record CustomerActivatedEvent(CustomerId CustomerId) : IDomainEvent;
