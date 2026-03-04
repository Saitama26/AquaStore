using Common.Domain.Errors;

namespace AquaStore.Domain.Errors;

/// <summary>
/// Ошибки, связанные с брендами
/// </summary>
public static class BrandErrors
{
    public static Error NotFound(Guid brandId) =>
        Error.NotFound("Brand.NotFound", $"Brand with ID '{brandId}' was not found");

    public static Error NotFoundBySlug(string slug) =>
        Error.NotFound("Brand.NotFoundBySlug", $"Brand with slug '{slug}' was not found");

    public static Error SlugAlreadyExists(string slug) =>
        Error.Conflict("Brand.SlugExists", $"Brand with slug '{slug}' already exists");

    public static Error CannotDeleteWithProducts =>
        Error.Conflict("Brand.HasProducts", "Cannot delete brand that has associated products");
}

