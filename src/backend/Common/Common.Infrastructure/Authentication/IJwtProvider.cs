using System.Security.Claims;

namespace Common.Infrastructure.Authentication;

/// <summary>
/// Интерфейс для работы с JWT токенами
/// </summary>
public interface IJwtProvider
{
    /// <summary>
    /// Сгенерировать access token
    /// </summary>
    string GenerateAccessToken(Guid userId, string email, IEnumerable<string> roles);

    /// <summary>
    /// Сгенерировать refresh token
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Получить principal из токена (без проверки срока действия)
    /// </summary>
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}

