namespace AquaStore.Contracts.Users.Requests;

/// <summary>
/// Запрос на обновление пользователя из админ-панели
/// </summary>
public sealed record AdminUpdateUserRequest(
    string FirstName,
    string LastName,
    string? Phone,
    string Role,
    bool IsActive);

