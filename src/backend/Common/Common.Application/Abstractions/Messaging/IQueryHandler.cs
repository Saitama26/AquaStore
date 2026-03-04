using Common.Domain.Results;
using MediatR;

namespace Common.Application.Abstractions.Messaging;

/// <summary>
/// Обработчик запроса
/// </summary>
/// <typeparam name="TQuery">Тип запроса</typeparam>
/// <typeparam name="TResponse">Тип ответа</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;

