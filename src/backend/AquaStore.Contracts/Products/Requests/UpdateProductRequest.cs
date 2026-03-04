namespace AquaStore.Contracts.Products.Requests;

/// <summary>
/// Запрос на обновление товара
/// </summary>
public sealed record UpdateProductRequest(
    string Name,
    string Description,
    string? ShortDescription,
    decimal Price,
    decimal? OldPrice,
    int FilterType,
    Guid CategoryId,
    Guid BrandId,
    string? Sku = null,
    int? FilterLifespanMonths = null,
    int? FilterCapacityLiters = null,
    double? FlowRateLitersPerMinute = null);

