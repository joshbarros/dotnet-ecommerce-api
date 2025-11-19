using Common.Domain;

namespace Modules.Catalog.Domain.Products;

// Product Domain Events

public sealed record ProductCreatedEvent(
    Guid ProductId,
    string Name,
    decimal Price,
    string Currency,
    Guid CategoryId) : DomainEvent;

public sealed record ProductUpdatedEvent(
    Guid ProductId,
    string Name) : DomainEvent;

public sealed record ProductPriceChangedEvent(
    Guid ProductId,
    decimal OldPrice,
    decimal NewPrice,
    string Currency) : DomainEvent;

public sealed record ProductActivatedEvent(
    Guid ProductId) : DomainEvent;

public sealed record ProductOutOfStockEvent(
    Guid ProductId) : DomainEvent;

public sealed record ProductDiscontinuedEvent(
    Guid ProductId) : DomainEvent;
