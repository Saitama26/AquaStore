using Common.Domain.Errors;

namespace AquaStore.Domain.Errors;

/// <summary>
/// Ошибки, связанные с товарами
/// </summary>
public static class ProductErrors
{
    public static Error NotFound(Guid productId) =>
        Error.NotFound("Product.NotFound", $"Product with ID '{productId}' was not found");

    public static Error NotFoundBySlug(string slug) =>
        Error.NotFound("Product.NotFoundBySlug", $"Product with slug '{slug}' was not found");

    public static Error SlugAlreadyExists(string slug) =>
        Error.Conflict("Product.SlugExists", $"Product with slug '{slug}' already exists");

    public static Error InsufficientStock(Guid productId, int requested, int available) =>
        Error.Validation("Product.InsufficientStock",
            $"Insufficient stock for product '{productId}'. Requested: {requested}, Available: {available}");

    public static Error InvalidPrice =>
        Error.Validation("Product.InvalidPrice", "Price must be greater than zero");

    public static Error InvalidQuantity =>
        Error.Validation("Product.InvalidQuantity", "Quantity must be greater than zero");
}

