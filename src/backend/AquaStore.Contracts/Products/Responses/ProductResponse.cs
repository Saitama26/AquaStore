namespace AquaStore.Contracts.Products.Responses;

/// <summary>
/// Краткая информация о товаре (для списков)
/// </summary>
public sealed record ProductResponse(
    Guid Id,
    string Name,
    string Slug,
    string? ShortDescription,
    decimal Price,
    decimal? OldPrice,
    string? MainImageUrl,
    string CategoryName,
    string BrandName,
    int FilterType,
    string FilterTypeName,
    int StockQuantity,
    bool IsInStock,
    bool IsFeatured,
    double? AverageRating,
    int ReviewCount);

