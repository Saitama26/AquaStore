namespace AquaStore.Contracts.Cart.Requests;

/// <summary>
/// Запрос на обновление количества в корзине
/// </summary>
public sealed record UpdateCartItemRequest(int Quantity);

