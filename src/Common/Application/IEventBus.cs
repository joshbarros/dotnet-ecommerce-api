namespace Common.Application;

/// <summary>
/// Event bus for publishing integration events across modules/services
/// </summary>
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class;
}

/// <summary>
/// Base record for integration events
/// Integration events are used for cross-module/service communication
/// </summary>
public abstract record IntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
