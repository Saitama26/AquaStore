using Common.Domain.Events;

namespace Common.Domain.Primitives;

/// <summary>
/// Корень агрегата - сущность, которая может публиковать доменные события
/// </summary>
/// <typeparam name="TId">Тип идентификатора</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot(TId id) : base(id) { }

    protected AggregateRoot() { }

    /// <summary>
    /// Добавить доменное событие
    /// </summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Очистить все доменные события (после публикации)
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

/// <summary>
/// Корень агрегата с Guid идентификатором
/// </summary>
public abstract class AggregateRoot : AggregateRoot<Guid>
{
    protected AggregateRoot(Guid id) : base(id) { }
    protected AggregateRoot() { }
}

