namespace Common.Domain.Primitives;

/// <summary>
/// Интерфейс для сущностей с отслеживанием времени создания и обновления
/// </summary>
public interface IAuditableEntity
{
    DateTime CreatedAtUtc { get; }
    DateTime? UpdatedAtUtc { get; }
}

