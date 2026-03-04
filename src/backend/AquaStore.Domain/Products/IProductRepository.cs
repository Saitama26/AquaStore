using Common.Application.Abstractions.Data;

namespace AquaStore.Domain.Products;

/// <summary>
/// Репозиторий товаров
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    /// <summary>
    /// Получить товар по slug
    /// </summary>
    Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить товары по категории
    /// </summary>
    Task<IReadOnlyList<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить товары по бренду
    /// </summary>
    Task<IReadOnlyList<Product>> GetByBrandAsync(Guid brandId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить избранные товары
    /// </summary>
    Task<IReadOnlyList<Product>> GetFeaturedAsync(int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить существование по slug
    /// </summary>
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить IQueryable с загруженными связанными сущностями для запросов
    /// </summary>
    IQueryable<Product> GetQueryableWithIncludes();
}

