using Common.Application.Abstractions.Services;
using Common.Infrastructure.Authentication;
using Common.Infrastructure.Data.Interceptors;
using Common.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Infrastructure;

/// <summary>
/// Регистрация сервисов Common.Infrastructure
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Добавить сервисы Common.Infrastructure
    /// </summary>
    public static IServiceCollection AddCommonInfrastructure(this IServiceCollection services)
    {
        // Сервисы
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.AddSingleton<IEmailSender, SmtpEmailSender>();

        // Аутентификация
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtProvider, JwtProvider>();

        // Interceptors
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();
        services.AddScoped<DomainEventInterceptor>();

        // Memory Cache
        services.AddMemoryCache();

        // HttpContextAccessor - должен быть добавлен в API проекте:
        // services.AddHttpContextAccessor();

        return services;
    }
}

