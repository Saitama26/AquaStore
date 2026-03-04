namespace AquaStore.Contracts.Orders.Requests;

/// <summary>
/// Запрос на создание заказа
/// </summary>
public sealed record CreateOrderRequest(
    string City,
    string Street,
    string Building,
    string? Apartment,
    string PostalCode,
    string? CustomerNote = null,
    IReadOnlyList<Guid>? ProductIds = null,
    bool BuyNowSingleUnit = false);

