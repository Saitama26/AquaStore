using Common.Domain.Errors;

namespace Common.Domain.Results;

/// <summary>
/// Расширения для работы с Result
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Преобразовать результат
    /// </summary>
    public static Result<TOut> Map<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> mapper)
    {
        return result.IsSuccess
            ? Result.Success(mapper(result.Value))
            : Result.Failure<TOut>(result.Error);
    }

    /// <summary>
    /// Связать результаты
    /// </summary>
    public static Result<TOut> Bind<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Result<TOut>> func)
    {
        return result.IsSuccess
            ? func(result.Value)
            : Result.Failure<TOut>(result.Error);
    }

    /// <summary>
    /// Асинхронное связывание результатов
    /// </summary>
    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<Result<TOut>>> func)
    {
        return result.IsSuccess
            ? await func(result.Value)
            : Result.Failure<TOut>(result.Error);
    }

    /// <summary>
    /// Выполнить действие при успехе
    /// </summary>
    public static Result<TValue> Tap<TValue>(
        this Result<TValue> result,
        Action<TValue> action)
    {
        if (result.IsSuccess)
            action(result.Value);

        return result;
    }

    /// <summary>
    /// Выполнить асинхронное действие при успехе
    /// </summary>
    public static async Task<Result<TValue>> TapAsync<TValue>(
        this Result<TValue> result,
        Func<TValue, Task> action)
    {
        if (result.IsSuccess)
            await action(result.Value);

        return result;
    }

    /// <summary>
    /// Получить значение или альтернативу
    /// </summary>
    public static TValue GetValueOrDefault<TValue>(
        this Result<TValue> result,
        TValue defaultValue)
    {
        return result.IsSuccess ? result.Value : defaultValue;
    }

    /// <summary>
    /// Получить значение или выбросить исключение
    /// </summary>
    public static TValue GetValueOrThrow<TValue>(
        this Result<TValue> result)
    {
        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Message);

        return result.Value;
    }

    /// <summary>
    /// Обработать результат (Match pattern)
    /// </summary>
    public static TOut Match<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> onSuccess,
        Func<Error, TOut> onFailure)
    {
        return result.IsSuccess
            ? onSuccess(result.Value)
            : onFailure(result.Error);
    }

    /// <summary>
    /// Обработать результат без значения
    /// </summary>
    public static TOut Match<TOut>(
        this Result result,
        Func<TOut> onSuccess,
        Func<Error, TOut> onFailure)
    {
        return result.IsSuccess
            ? onSuccess()
            : onFailure(result.Error);
    }

    /// <summary>
    /// Убедиться в выполнении условия
    /// </summary>
    public static Result<TValue> Ensure<TValue>(
        this Result<TValue> result,
        Func<TValue, bool> predicate,
        Error error)
    {
        if (result.IsFailure)
            return result;

        return predicate(result.Value)
            ? result
            : Result.Failure<TValue>(error);
    }

    /// <summary>
    /// Комбинировать несколько результатов
    /// </summary>
    public static Result Combine(params Result[] results)
    {
        var failedResult = results.FirstOrDefault(r => r.IsFailure);
        return failedResult ?? Result.Success();
    }

    /// <summary>
    /// Комбинировать несколько результатов с ошибками
    /// </summary>
    public static Result CombineAll(params Result[] results)
    {
        var errors = results
            .Where(r => r.IsFailure)
            .Select(r => r.Error)
            .ToArray();

        return errors.Length == 0
            ? Result.Success()
            : ValidationResult.WithErrors(errors);
    }
}

