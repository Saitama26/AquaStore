namespace Common.Application.Exceptions;

/// <summary>
/// Исключение "сущность не найдена"
/// </summary>
public sealed class NotFoundException : ApplicationException
{
    public NotFoundException(string entityName, object key)
        : base(
            "Entity.NotFound",
            $"{entityName} with key '{key}' was not found.")
    {
    }
}

