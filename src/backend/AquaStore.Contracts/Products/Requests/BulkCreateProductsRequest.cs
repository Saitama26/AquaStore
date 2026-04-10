namespace AquaStore.Contracts.Products.Requests;

/// <summary>
/// Запрос на массовое создание товаров
/// </summary>
public sealed record BulkCreateProductsRequest(
    IReadOnlyList<BulkCreateProductItemRequest> Products);

public sealed record BulkCreateProductItemRequest(
    string Name,
    string Description,
    string? ShortDescription,
    decimal Price,
    decimal? OldPrice,
    int FilterType,
    string CategoryName,
    string BrandName,
    int StockQuantity = 0,
    string? Sku = null,
    int? FilterLifespanMonths = null,
    int? FilterCapacityLiters = null,
    double? FlowRateLitersPerMinute = null,
    List<string>? ImageUrls = null);
