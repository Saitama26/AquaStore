namespace AquaStore.Contracts.Auth.Responses;

/// <summary>
/// Ответ аутентификации
/// </summary>
public sealed record AuthResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt);

