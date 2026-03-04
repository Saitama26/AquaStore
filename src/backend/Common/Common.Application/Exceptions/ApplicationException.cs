namespace Common.Application.Exceptions;

/// <summary>
/// Базовое исключение уровня приложения
/// </summary>
public class ApplicationException : Exception
{
    public string Code { get; }

    public ApplicationException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    public ApplicationException(string code, string message, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }
}

