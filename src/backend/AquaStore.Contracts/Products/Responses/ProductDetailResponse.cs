namespace AquaStore.Contracts.Products.Responses;

/// <summary>
/// Полная информация о товаре
/// </summary>
public sealed record ProductDetailResponse(
    Guid Id,
    string Name,
    string Slug,
    string Description,
    string? ShortDescription,
    decimal Price,
    decimal? OldPrice,
    string Currency,
    int FilterType,
    string FilterTypeName,
    int StockQuantity,
    string? Sku,
    bool IsActive,
    bool IsFeatured,
    ProductSpecificationsResponse? Specifications,
    Guid CategoryId,
    string CategoryName,
    Guid BrandId,
    string BrandName,
    IReadOnlyList<ProductImageResponse> Images,
    double? AverageRating,
    int ReviewCount,
    DateTime CreatedAt);

/// <summary>
/// Характеристики фильтра
/// </summary>
public sealed record ProductSpecificationsResponse(
    int? FilterLifespanMonths,
    int? FilterCapacityLiters,
    double? FlowRateLitersPerMinute);

/// <summary>
/// Изображение товара
/// </summary>
public sealed record ProductImageResponse(
    Guid Id,
    string Url,
    string? AltText,
    bool IsMain,
    int SortOrder);

