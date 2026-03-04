namespace AquaStore.Contracts.Cart.Requests;

/// <summary>
/// Запрос на добавление в корзину
/// </summary>
public sealed record AddToCartRequest(
    Guid ProductId,
    int Quantity = 1);

