namespace AquaStore.Contracts.Users.Responses;

/// <summary>
/// Пользователь для отображения в админ-панели
/// </summary>
public sealed record AdminUserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? Phone,
    string Role,
    bool IsActive,
    DateTime CreatedAt);

