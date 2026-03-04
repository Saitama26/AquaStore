namespace AquaStore.Contracts.Cart.Responses;

/// <summary>
/// Информация о корзине
/// </summary>
public sealed record CartResponse(
    Guid Id,
    Guid UserId,
    IReadOnlyList<CartItemResponse> Items,
    decimal TotalAmount,
    string Currency,
    int TotalItems);

/// <summary>
/// Элемент корзины
/// </summary>
public sealed record CartItemResponse(
    Guid ProductId,
    string ProductName,
    string ProductSlug,
    string? ProductImageUrl,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice);

