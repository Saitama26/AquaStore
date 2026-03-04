namespace AquaStore.Contracts.Brands.Requests;

/// <summary>
/// Запрос на обновление бренда
/// </summary>
public sealed record UpdateBrandRequest(
    string Name,
    string? Description,
    string? Country,
    string? LogoUrl,
    string? WebsiteUrl);

