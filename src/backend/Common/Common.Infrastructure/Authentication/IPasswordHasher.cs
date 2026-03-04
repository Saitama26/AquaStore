namespace Common.Infrastructure.Authentication;

/// <summary>
/// Интерфейс для хэширования паролей
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Хэшировать пароль
    /// </summary>
    string Hash(string password);

    /// <summary>
    /// Проверить пароль
    /// </summary>
    bool Verify(string password, string passwordHash);
}

