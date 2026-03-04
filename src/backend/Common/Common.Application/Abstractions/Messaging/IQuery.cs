using Common.Domain.Results;
using MediatR;

namespace Common.Application.Abstractions.Messaging;

/// <summary>
/// Запрос с возвращаемым значением
/// </summary>
/// <typeparam name="TResponse">Тип ответа</typeparam>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;

