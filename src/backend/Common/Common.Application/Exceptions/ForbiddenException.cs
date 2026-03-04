namespace Common.Application.Exceptions;

/// <summary>
/// Исключение "доступ запрещён"
/// </summary>
public sealed class ForbiddenException : ApplicationException
{
    public ForbiddenException(string message = "Access to this resource is forbidden")
        : base("Access.Forbidden", message)
    {
    }
}

