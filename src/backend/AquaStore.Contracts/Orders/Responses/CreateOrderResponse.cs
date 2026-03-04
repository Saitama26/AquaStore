namespace AquaStore.Contracts.Orders.Responses;

/// <summary>
/// Ответ на создание заказа
/// </summary>
public sealed record CreateOrderResponse(
    Guid OrderId,
    string OrderNumber);

