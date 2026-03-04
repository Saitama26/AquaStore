using Common.Domain.Errors;

namespace Common.Domain.Results;

/// <summary>
/// Результат операции без возвращаемого значения
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("Success result cannot have an error");

        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("Failure result must have an error");

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Создать успешный результат
    /// </summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Создать результат с ошибкой
    /// </summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Создать успешный результат с значением
    /// </summary>
    public static Result<TValue> Success<TValue>(TValue value) => 
        Result<TValue>.Success(value);

    /// <summary>
    /// Создать результат с ошибкой для типизированного результата
    /// </summary>
    public static Result<TValue> Failure<TValue>(Error error) => 
        Result<TValue>.Failure(error);

    /// <summary>
    /// Создать результат на основе условия
    /// </summary>
    public static Result Create(bool condition, Error error) =>
        condition ? Success() : Failure(error);

    /// <summary>
    /// Создать типизированный результат на основе условия
    /// </summary>
    public static Result<TValue> Create<TValue>(TValue? value, Error error) =>
        value is not null ? Success(value) : Failure<TValue>(error);
}

/// <summary>
/// Результат операции с возвращаемым значением
/// </summary>
/// <typeparam name="TValue">Тип возвращаемого значения</typeparam>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value of a failed result");

    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Создать успешный результат
    /// </summary>
    public static Result<TValue> Success(TValue value) => 
        new(value, true, Error.None);

    /// <summary>
    /// Создать результат с ошибкой
    /// </summary>
    public new static Result<TValue> Failure(Error error) => 
        new(default, false, error);

    /// <summary>
    /// Неявное преобразование из значения в Result
    /// </summary>
    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure(Error.NullValue);

    /// <summary>
    /// Неявное преобразование из Error в Result
    /// </summary>
    public static implicit operator Result<TValue>(Error error) => Failure(error);
}

