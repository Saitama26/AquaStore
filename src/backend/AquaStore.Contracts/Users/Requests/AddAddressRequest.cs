namespace AquaStore.Contracts.Users.Requests;

/// <summary>
/// Запрос на добавление адреса
/// </summary>
public sealed record AddAddressRequest(
    string City,
    string Street,
    string Building,
    string? Apartment,
    string PostalCode,
    bool IsDefault = false);

