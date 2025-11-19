using Common.Domain;

namespace Modules.Orders.Domain.Orders.Events;

public sealed record OrderConfirmedEvent(OrderId OrderId) : IDomainEvent;
