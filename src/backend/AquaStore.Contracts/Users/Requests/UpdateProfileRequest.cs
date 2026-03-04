namespace AquaStore.Contracts.Users.Requests;

/// <summary>
/// Запрос на обновление профиля
/// </summary>
public sealed record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string? Phone);

