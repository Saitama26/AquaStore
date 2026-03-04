using Common.Domain.Errors;

namespace AquaStore.Domain.Errors;

/// <summary>
/// Ошибки, связанные с корзиной
/// </summary>
public static class CartErrors
{
    public static Error NotFound(Guid cartId) =>
        Error.NotFound("Cart.NotFound", $"Cart with ID '{cartId}' was not found");

    public static Error ItemNotFound(Guid productId) =>
        Error.NotFound("Cart.ItemNotFound", $"Item with product ID '{productId}' not found in cart");

    public static Error IsEmpty =>
        Error.Validation("Cart.IsEmpty", "Cart is empty");

    public static Error InvalidQuantity =>
        Error.Validation("Cart.InvalidQuantity", "Quantity must be greater than zero");
}

