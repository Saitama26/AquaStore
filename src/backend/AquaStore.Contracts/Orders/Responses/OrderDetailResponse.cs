namespace AquaStore.Contracts.Orders.Responses;

/// <summary>
/// Полная информация о заказе
/// </summary>
public sealed record OrderDetailResponse(
    Guid Id,
    string OrderNumber,
    int Status,
    string StatusName,
    int PaymentStatus,
    string PaymentStatusName,
    OrderAddressResponse ShippingAddress,
    string? CustomerNote,
    decimal SubTotal,
    decimal ShippingCost,
    decimal? Discount,
    decimal TotalAmount,
    string Currency,
    IReadOnlyList<OrderItemResponse> Items,
    DateTime? ShippedAt,
    DateTime? DeliveredAt,
    DateTime CreatedAt);

/// <summary>
/// Адрес доставки
/// </summary>
public sealed record OrderAddressResponse(
    string City,
    string Street,
    string Building,
    string? Apartment,
    string PostalCode);

/// <summary>
/// Позиция заказа
/// </summary>
public sealed record OrderItemResponse(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice);

