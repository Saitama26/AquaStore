using Common.Domain.Events;
using MediatR;

namespace Common.Application.Abstractions.Messaging;

/// <summary>
/// Обработчик доменного события
/// </summary>
/// <typeparam name="TEvent">Тип события</typeparam>
public interface IDomainEventHandler<in TEvent> : INotificationHandler<TEvent>
    where TEvent : IDomainEvent;

