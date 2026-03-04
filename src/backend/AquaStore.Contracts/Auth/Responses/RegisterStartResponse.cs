namespace AquaStore.Contracts.Auth.Responses;

/// <summary>
/// Ответ на старт регистрации
/// </summary>
public sealed record RegisterStartResponse(
    string Email,
    int ExpiresInMinutes);

