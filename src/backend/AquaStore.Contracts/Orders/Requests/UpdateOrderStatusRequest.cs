namespace AquaStore.Contracts.Orders.Requests;

/// <summary>
/// Запрос обновления статуса заказа (для администратора)
/// </summary>
public sealed record UpdateOrderStatusRequest(int Status);
