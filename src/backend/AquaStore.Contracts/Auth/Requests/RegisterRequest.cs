namespace AquaStore.Contracts.Auth.Requests;

/// <summary>
/// Запрос на регистрацию
/// </summary>
public sealed record RegisterRequest(
    string Email,
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName,
    string Phone);

