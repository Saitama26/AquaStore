namespace Common.Application.Abstractions.Services;

/// <summary>
/// Сервис получения информации о текущем пользователе
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Идентификатор текущего пользователя
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Email текущего пользователя
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Аутентифицирован ли пользователь
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Роли пользователя
    /// </summary>
    IEnumerable<string> Roles { get; }

    /// <summary>
    /// Проверить наличие роли
    /// </summary>
    bool IsInRole(string role);
}

