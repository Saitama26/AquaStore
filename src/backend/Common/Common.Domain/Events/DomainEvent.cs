namespace Common.Domain.Events;

/// <summary>
/// Базовый класс для доменных событий
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

