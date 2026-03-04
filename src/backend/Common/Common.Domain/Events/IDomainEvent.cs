using MediatR;

namespace Common.Domain.Events;

/// <summary>
/// Маркерный интерфейс для доменных событий
/// </summary>
public interface IDomainEvent : INotification
{
    Guid Id { get; }
    DateTime OccurredOnUtc { get; }
}

