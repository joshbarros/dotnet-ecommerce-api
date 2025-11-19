namespace Common.Domain;

/// <summary>
/// Marker interface for domain events
/// Domain events represent something that happened in the domain
/// </summary>
public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}

/// <summary>
/// Base record for domain events
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
