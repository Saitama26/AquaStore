namespace AquaStore.Contracts.Categories.Requests;

/// <summary>
/// Запрос на обновление категории
/// </summary>
public sealed record UpdateCategoryRequest(
    string Name,
    string? Description,
    string? ImageUrl);

