namespace AquaStore.Contracts.Products.Requests;

/// <summary>
/// Запрос на создание товара
/// </summary>
public sealed record CreateProductRequest(
    string Name,
    string Description,
    string? ShortDescription,
    decimal Price,
    decimal? OldPrice,
    int FilterType,
    Guid CategoryId,
    Guid BrandId,
    int StockQuantity = 0,
    string? Sku = null,
    int? FilterLifespanMonths = null,
    int? FilterCapacityLiters = null,
    double? FlowRateLitersPerMinute = null,
    List<string>? ImageUrls = null);

