namespace AquaStore.Contracts.Orders.Responses;

/// <summary>
/// Краткая информация о заказе (для списков)
/// </summary>
public sealed record OrderResponse(
    Guid Id,
    string OrderNumber,
    int Status,
    string StatusName,
    int PaymentStatus,
    string PaymentStatusName,
    decimal TotalAmount,
    string Currency,
    int ItemCount,
    DateTime CreatedAt);

