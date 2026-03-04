using Common.Application.Abstractions.Data;

namespace AquaStore.Domain.Orders;

/// <summary>
/// Репозиторий заказов
/// </summary>
public interface IOrderRepository : IRepository<Order>
{
    /// <summary>
    /// Получить заказ по номеру
    /// </summary>
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить заказы пользователя
    /// </summary>
    Task<IReadOnlyList<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить заказ со всеми связанными данными
    /// </summary>
    Task<Order?> GetWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить все заказы (для админа)
    /// </summary>
    Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default);
}

