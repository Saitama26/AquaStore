namespace AquaStore.Contracts.Users.Responses;

/// <summary>
/// Профиль пользователя
/// </summary>
public sealed record UserProfileResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? Phone,
    string Role,
    bool EmailConfirmed,
    IReadOnlyList<UserAddressResponse> Addresses,
    DateTime CreatedAt);

/// <summary>
/// Адрес пользователя
/// </summary>
public sealed record UserAddressResponse(
    Guid Id,
    string City,
    string Street,
    string Building,
    string? Apartment,
    string PostalCode,
    bool IsDefault);

