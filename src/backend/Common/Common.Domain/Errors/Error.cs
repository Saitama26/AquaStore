namespace Common.Domain.Errors;

/// <summary>
/// Представление ошибки в системе
/// </summary>
public sealed record Error
{
    public string Code { get; }
    public string Message { get; }
    public ErrorType Type { get; }

    private Error(string code, string message, ErrorType type)
    {
        Code = code;
        Message = message;
        Type = type;
    }

    /// <summary>
    /// Отсутствие ошибки
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);

    /// <summary>
    /// Null значение
    /// </summary>
    public static readonly Error NullValue = new(
        "Error.NullValue",
        "Null value was provided",
        ErrorType.Failure);

    /// <summary>
    /// Создать ошибку валидации
    /// </summary>
    public static Error Validation(string code, string message) =>
        new(code, message, ErrorType.Validation);

    /// <summary>
    /// Создать ошибку "не найдено"
    /// </summary>
    public static Error NotFound(string code, string message) =>
        new(code, message, ErrorType.NotFound);

    /// <summary>
    /// Создать ошибку конфликта
    /// </summary>
    public static Error Conflict(string code, string message) =>
        new(code, message, ErrorType.Conflict);

    /// <summary>
    /// Создать ошибку доступа
    /// </summary>
    public static Error Forbidden(string code, string message) =>
        new(code, message, ErrorType.Forbidden);

    /// <summary>
    /// Создать ошибку авторизации
    /// </summary>
    public static Error Unauthorized(string code, string message) =>
        new(code, message, ErrorType.Unauthorized);

    /// <summary>
    /// Создать общую ошибку
    /// </summary>
    public static Error Failure(string code, string message) =>
        new(code, message, ErrorType.Failure);

    /// <summary>
    /// Создать ошибку из исключения
    /// </summary>
    public static Error FromException(Exception exception) =>
        new("Error.Exception", exception.Message, ErrorType.Failure);
}

