using Common.Domain;

namespace Modules.Customers.Domain.Customers.Events;

public sealed record CustomerDeletedEvent(CustomerId CustomerId) : IDomainEvent;
