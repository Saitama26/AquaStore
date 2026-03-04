namespace AquaStore.Contracts.Auth.Requests;

/// <summary>
/// Запрос на обновление токена
/// </summary>
public sealed record RefreshTokenRequest(
    string AccessToken,
    string RefreshToken);

