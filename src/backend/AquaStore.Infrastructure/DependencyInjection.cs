using AquaStore.Domain.Brands;
using AquaStore.Domain.Cart;
using AquaStore.Domain.Categories;
using AquaStore.Domain.Orders;
using AquaStore.Domain.Products;
using AquaStore.Domain.Reviews;
using AquaStore.Domain.Users;
using AquaStore.Infrastructure.Data;
using AquaStore.Infrastructure.Repositories;
using Common.Application.Abstractions.Data;
using Common.Infrastructure;
using Common.Infrastructure.Data.Interceptors;
using Common.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace AquaStore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAquaStoreInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add Common Infrastructure
        services.AddCommonInfrastructure();

        // Database
        services.AddDbContext<AquaStoreDbContext>((sp, options) =>
        {
            // Читаем connection string из переменных окружения или конфига
            var dbHost = Environment.GetEnvironmentVariable("DB_HOST") 
                ?? configuration["ConnectionStrings:DefaultConnection"]?.Split(';')
                    .FirstOrDefault(s => s.StartsWith("Server="))?.Split('=')[1]?.Trim()
                ?? "localhost";
            
            var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "3306";
            var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "aquastore";
            var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "root";
            var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "yourpassword";

            // Формируем connection string из переменных окружения или используем из конфига
            var connectionString = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DB_HOST"))
                ? $"Server={dbHost};Port={dbPort};Database={dbName};User={dbUser};Password={dbPassword};"
                : configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            // Для Docker используем ServerVersion.Parse, для локальной разработки можно AutoDetect
            ServerVersion serverVersion;
            var mysqlVersion = Environment.GetEnvironmentVariable("MYSQL_VERSION");
            
            if (!string.IsNullOrEmpty(mysqlVersion))
            {
                // В Docker контейнере используем указанную версию
                serverVersion = ServerVersion.Parse(mysqlVersion, ServerType.MySql);
            }
            else
            {
                try
                {
                    // Попытка автоопределения (работает только если MySQL доступен)
                    serverVersion = ServerVersion.AutoDetect(connectionString);
                }
                catch
                {
                    // Fallback на версию 8.0
                    serverVersion = ServerVersion.Parse("8.0.0-mysql", ServerType.MySql);
                }
            }
            
            options.UseMySql(connectionString, serverVersion, mysqlOptions =>
            {
                mysqlOptions.MigrationsAssembly(typeof(AquaStoreDbContext).Assembly.FullName);
                // Отключаем retry для транзакций, чтобы избежать конфликта с пользовательскими транзакциями
                // Retry будет работать на уровне отдельных запросов через execution strategy
            });

            // Add interceptors
            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<DomainEventInterceptor>());
        });

        // Unit of Work
        services.AddScoped<IUnitOfWork>(sp => 
            new UnitOfWork<AquaStoreDbContext>(sp.GetRequiredService<AquaStoreDbContext>()));

        // Repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }
}
