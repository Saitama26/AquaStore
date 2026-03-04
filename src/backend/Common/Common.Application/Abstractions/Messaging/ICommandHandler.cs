using Common.Domain.Results;
using MediatR;

namespace Common.Application.Abstractions.Messaging;

/// <summary>
/// Обработчик команды без возвращаемого значения
/// </summary>
/// <typeparam name="TCommand">Тип команды</typeparam>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand;

/// <summary>
/// Обработчик команды с возвращаемым значением
/// </summary>
/// <typeparam name="TCommand">Тип команды</typeparam>
/// <typeparam name="TResponse">Тип ответа</typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;

