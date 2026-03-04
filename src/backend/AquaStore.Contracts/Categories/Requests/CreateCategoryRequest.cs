namespace AquaStore.Contracts.Categories.Requests;

/// <summary>
/// Запрос на создание категории
/// </summary>
public sealed record CreateCategoryRequest(
    string Name,
    string? Description = null,
    Guid? ParentCategoryId = null,
    string? ImageUrl = null);

