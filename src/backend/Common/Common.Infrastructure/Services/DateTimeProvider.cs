using Common.Application.Abstractions.Services;

namespace Common.Infrastructure.Services;

/// <summary>
/// Реализация провайдера текущего времени
/// </summary>
public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}

