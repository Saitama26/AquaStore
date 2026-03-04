namespace AquaStore.Contracts.Categories.Responses;

/// <summary>
/// Информация о категории
/// </summary>
public sealed record CategoryResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl,
    Guid? ParentCategoryId,
    bool IsActive,
    IReadOnlyList<CategoryResponse> SubCategories);

