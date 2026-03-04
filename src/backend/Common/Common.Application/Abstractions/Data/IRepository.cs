using System.Linq.Expressions;
using Common.Domain.Primitives;

namespace Common.Application.Abstractions.Data;

/// <summary>
/// Базовый интерфейс репозитория
/// </summary>
/// <typeparam name="TEntity">Тип сущности</typeparam>
/// <typeparam name="TId">Тип идентификатора</typeparam>
public interface IRepository<TEntity, in TId>
    where TEntity : Entity<TId>
    where TId : notnull
{
    /// <summary>
    /// Получить сущность по Id
    /// </summary>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить все сущности
    /// </summary>
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Найти сущности по условию
    /// </summary>
    Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить существование по условию
    /// </summary>
    Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавить сущность
    /// </summary>
    void Add(TEntity entity);

    /// <summary>
    /// Добавить несколько сущностей
    /// </summary>
    void AddRange(IEnumerable<TEntity> entities);

    /// <summary>
    /// Обновить сущность
    /// </summary>
    void Update(TEntity entity);

    /// <summary>
    /// Удалить сущность
    /// </summary>
    void Remove(TEntity entity);

    /// <summary>
    /// Удалить несколько сущностей
    /// </summary>
    void RemoveRange(IEnumerable<TEntity> entities);
}

/// <summary>
/// Репозиторий с Guid идентификатором
/// </summary>
public interface IRepository<TEntity> : IRepository<TEntity, Guid>
    where TEntity : Entity<Guid>;

