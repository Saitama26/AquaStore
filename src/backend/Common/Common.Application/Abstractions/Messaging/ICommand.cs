using Common.Domain.Results;
using MediatR;

namespace Common.Application.Abstractions.Messaging;

/// <summary>
/// Команда без возвращаемого значения
/// </summary>
public interface ICommand : IRequest<Result>, IBaseCommand;

/// <summary>
/// Команда с возвращаемым значением
/// </summary>
/// <typeparam name="TResponse">Тип ответа</typeparam>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand;

/// <summary>
/// Маркерный интерфейс для команд
/// </summary>
public interface IBaseCommand;

