namespace Common.Infrastructure.Authentication;

/// <summary>
/// Настройки JWT
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>
    /// Секретный ключ
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// Издатель токена
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Аудитория токена
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Время жизни access token в минутах
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 15;

    /// <summary>
    /// Время жизни refresh token в днях
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;
}

