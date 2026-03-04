namespace AquaStore.Contracts.Brands.Responses;

/// <summary>
/// Информация о бренде
/// </summary>
public sealed record BrandResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? Country,
    string? LogoUrl,
    string? WebsiteUrl,
    bool IsActive);

