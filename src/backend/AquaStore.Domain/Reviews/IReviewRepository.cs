using Common.Application.Abstractions.Data;

namespace AquaStore.Domain.Reviews;

/// <summary>
/// Репозиторий отзывов
/// </summary>
public interface IReviewRepository : IRepository<Review>
{
    /// <summary>
    /// Получить отзывы по товару
    /// </summary>
    Task<IReadOnlyList<Review>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить отзывы по товару с пагинацией
    /// </summary>
    Task<(IReadOnlyList<Review> Items, int TotalCount)> GetPagedByProductIdAsync(
        Guid productId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить отзывы пользователя
    /// </summary>
    Task<IReadOnlyList<Review>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить, оставлял ли пользователь отзыв на товар
    /// </summary>
    Task<bool> UserHasReviewedProductAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить средний рейтинг товара
    /// </summary>
    Task<double?> GetAverageRatingAsync(Guid productId, CancellationToken cancellationToken = default);
}

