using Common.Application.Abstractions.Data;

namespace AquaStore.Domain.Categories;

/// <summary>
/// Репозиторий категорий
/// </summary>
public interface ICategoryRepository : IRepository<Category>
{
    /// <summary>
    /// Получить категорию по slug
    /// </summary>
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить корневые категории (без родителя)
    /// </summary>
    Task<IReadOnlyList<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить подкатегории
    /// </summary>
    Task<IReadOnlyList<Category>> GetSubCategoriesAsync(Guid parentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить существование по slug
    /// </summary>
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
}

