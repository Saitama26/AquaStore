namespace Common.Domain.Primitives;

/// <summary>
/// Интерфейс для сущностей с мягким удалением
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; }
    DateTime? DeletedAtUtc { get; }
    
    void Delete();
    void Restore();
}

