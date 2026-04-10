namespace AquaStore.Contracts.Categories.Requests;

/// <summary>
/// Запрос на массовое создание категорий
/// </summary>
public sealed record BulkCreateCategoriesRequest(
    IReadOnlyList<BulkCreateCategoryItemRequest> Categories);

public sealed record BulkCreateCategoryItemRequest(
    string Name,
    string? Description = null,
    string? ImageUrl = null);
