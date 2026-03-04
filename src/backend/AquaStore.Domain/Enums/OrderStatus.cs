namespace AquaStore.Domain.Enums;

/// <summary>
/// Статусы заказа
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Ожидает обработки
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Подтверждён
    /// </summary>
    Confirmed = 1,

    /// <summary>
    /// В обработке
    /// </summary>
    Processing = 2,

    /// <summary>
    /// Отправлен
    /// </summary>
    Shipped = 3,

    /// <summary>
    /// Доставлен
    /// </summary>
    Delivered = 4,

    /// <summary>
    /// Отменён
    /// </summary>
    Cancelled = 5
}

