namespace AquaStore.Contracts.Auth.Requests;

/// <summary>
/// Запрос на вход
/// </summary>
public sealed record LoginRequest(
    string Email,
    string Password);

