using Common.Application.Abstractions.Data;

namespace AquaStore.Domain.Cart;

/// <summary>
/// Репозиторий корзин
/// </summary>
public interface ICartRepository : IRepository<Cart>
{
    /// <summary>
    /// Получить корзину пользователя
    /// </summary>
    Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить корзину с товарами
    /// </summary>
    Task<Cart?> GetWithItemsAsync(Guid cartId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить или создать корзину для пользователя
    /// </summary>
    Task<Cart> GetOrCreateAsync(Guid userId, CancellationToken cancellationToken = default);
}

