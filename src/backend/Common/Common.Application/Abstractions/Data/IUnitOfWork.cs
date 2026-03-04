namespace Common.Application.Abstractions.Data;

/// <summary>
/// Паттерн Unit of Work для управления транзакциями
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Сохранить все изменения в базе данных
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Начать транзакцию
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Зафиксировать транзакцию
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Откатить транзакцию
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

