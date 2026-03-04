using Common.Application;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AquaStore.Application;

/// <summary>
/// Регистрация сервисов AquaStore.Application
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Добавить сервисы AquaStore.Application
    /// </summary>
    public static IServiceCollection AddAquaStoreApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Регистрируем Common.Application с текущей сборкой
        services.AddCommonApplication(assembly);

        return services;
    }
}

