namespace Common.Application.Abstractions.Services;

/// <summary>
/// Провайдер текущего времени (для тестируемости)
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Текущее время UTC
    /// </summary>
    DateTime UtcNow { get; }
}

