namespace AquaStore.Contracts.Brands.Requests;

/// <summary>
/// Запрос на создание бренда
/// </summary>
public sealed record CreateBrandRequest(
    string Name,
    string? Description = null,
    string? Country = null,
    string? LogoUrl = null,
    string? WebsiteUrl = null);

