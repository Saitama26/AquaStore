using AquaStore.Contracts.Common;

namespace AquaStore.Contracts.Products.Requests;

/// <summary>
/// Запрос на фильтрацию товаров
/// </summary>
public sealed record ProductFilterRequest : PagedRequest
{
    public string? SearchTerm { get; init; }
    public Guid? CategoryId { get; init; }
    public Guid? BrandId { get; init; }
    public int? FilterType { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public bool? InStock { get; init; }
    public bool? IsFeatured { get; init; }
}

