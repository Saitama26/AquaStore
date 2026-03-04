namespace Common.Application.Exceptions;

/// <summary>
/// Исключение конфликта данных
/// </summary>
public sealed class ConflictException : ApplicationException
{
    public ConflictException(string entityName, string message)
        : base(
            "Entity.Conflict",
            $"Conflict occurred for {entityName}: {message}")
    {
    }
}

