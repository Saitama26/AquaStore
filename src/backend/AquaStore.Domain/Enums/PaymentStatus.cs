namespace AquaStore.Domain.Enums;

/// <summary>
/// Статусы оплаты
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Ожидает оплаты
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Оплачен
    /// </summary>
    Paid = 1,

    /// <summary>
    /// Ошибка оплаты
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Возврат средств
    /// </summary>
    Refunded = 3
}

