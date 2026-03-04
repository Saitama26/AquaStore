using Common.Domain.Errors;

namespace Common.Domain.Results;

/// <summary>
/// Результат валидации с множественными ошибками
/// </summary>
public sealed class ValidationResult : Result, IValidationResult
{
    public Error[] Errors { get; }

    private ValidationResult(Error[] errors)
        : base(false, IValidationResult.ValidationError)
    {
        Errors = errors;
    }

    /// <summary>
    /// Создать результат валидации с ошибками
    /// </summary>
    public static ValidationResult WithErrors(Error[] errors) => new(errors);
}

/// <summary>
/// Типизированный результат валидации
/// </summary>
public sealed class ValidationResult<TValue> : Result<TValue>, IValidationResult
{
    public Error[] Errors { get; }

    private ValidationResult(Error[] errors)
        : base(default, false, IValidationResult.ValidationError)
    {
        Errors = errors;
    }

    /// <summary>
    /// Создать результат валидации с ошибками
    /// </summary>
    public static ValidationResult<TValue> WithErrors(Error[] errors) => new(errors);
}

/// <summary>
/// Интерфейс результата валидации
/// </summary>
public interface IValidationResult
{
    public static readonly Error ValidationError = Error.Validation(
        "Validation.Error",
        "One or more validation errors occurred");

    Error[] Errors { get; }
}

