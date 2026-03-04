using Common.Application.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Common.Application;

/// <summary>
/// Регистрация сервисов Common.Application
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Добавить сервисы Common.Application
    /// </summary>
    public static IServiceCollection AddCommonApplication(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        // Регистрация MediatR с behaviors
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblies(assemblies);
            
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });

        // Регистрация валидаторов
        services.AddValidatorsFromAssemblies(assemblies, includeInternalTypes: true);

        return services;
    }
}

