namespace AquaStore.Contracts.Brands.Requests;

/// <summary>
/// Запрос на массовое создание брендов
/// </summary>
public sealed record BulkCreateBrandsRequest(
    IReadOnlyList<BulkCreateBrandItemRequest> Brands);

public sealed record BulkCreateBrandItemRequest(
    string Name,
    string? Description = null,
    string? Country = null,
    string? LogoUrl = null,
    string? WebsiteUrl = null);
