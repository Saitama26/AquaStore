namespace AquaStore.Contracts.Auth.Requests;

/// <summary>
/// Запрос на подтверждение регистрации
/// </summary>
public sealed record ConfirmRegistrationRequest(
    string Email,
    string Code);

