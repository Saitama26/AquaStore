using Common.Application.Abstractions.Data;

namespace AquaStore.Domain.Brands;

/// <summary>
/// Репозиторий брендов
/// </summary>
public interface IBrandRepository : IRepository<Brand>
{
    /// <summary>
    /// Получить бренд по slug
    /// </summary>
    Task<Brand?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить существование по slug
    /// </summary>
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
}

