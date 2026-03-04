using Common.Domain.Errors;

namespace AquaStore.Domain.Errors;

/// <summary>
/// Ошибки, связанные с категориями
/// </summary>
public static class CategoryErrors
{
    public static Error NotFound(Guid categoryId) =>
        Error.NotFound("Category.NotFound", $"Category with ID '{categoryId}' was not found");

    public static Error NotFoundBySlug(string slug) =>
        Error.NotFound("Category.NotFoundBySlug", $"Category with slug '{slug}' was not found");

    public static Error SlugAlreadyExists(string slug) =>
        Error.Conflict("Category.SlugExists", $"Category with slug '{slug}' already exists");

    public static Error CannotDeleteWithProducts =>
        Error.Conflict("Category.HasProducts", "Cannot delete category that contains products");

    public static Error CannotDeleteWithSubcategories =>
        Error.Conflict("Category.HasSubcategories", "Cannot delete category that has subcategories");

    public static Error CircularReference =>
        Error.Validation("Category.CircularReference", "Cannot set parent category that would create a circular reference");
}

