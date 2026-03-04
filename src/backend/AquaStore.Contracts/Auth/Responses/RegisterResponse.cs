namespace AquaStore.Contracts.Auth.Responses;

/// <summary>
/// Ответ регистрации
/// </summary>
public sealed record RegisterResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName);

