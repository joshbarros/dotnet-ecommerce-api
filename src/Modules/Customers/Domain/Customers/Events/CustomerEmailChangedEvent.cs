using Common.Domain;
using Modules.Customers.Domain.Common;

namespace Modules.Customers.Domain.Customers.Events;

public sealed record CustomerEmailChangedEvent(
    CustomerId CustomerId,
    Email OldEmail,
    Email NewEmail) : IDomainEvent;
