using System.Diagnostics;
using Common.Domain.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Common.Application.Behaviors;

/// <summary>
/// Pipeline behavior для логирования запросов
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation(
            "Processing request {RequestName}",
            requestName);

        var stopwatch = Stopwatch.StartNew();

        var result = await next();

        stopwatch.Stop();

        if (result.IsFailure)
        {
            _logger.LogWarning(
                "Request {RequestName} failed with error {Error} in {ElapsedMs}ms",
                requestName,
                result.Error.Message,
                stopwatch.ElapsedMilliseconds);
        }
        else
        {
            _logger.LogInformation(
                "Request {RequestName} completed successfully in {ElapsedMs}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);
        }

        return result;
    }
}

