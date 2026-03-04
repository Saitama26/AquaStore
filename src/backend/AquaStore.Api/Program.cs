using AquaStore.Api.Extensions;
using AquaStore.Api.Middleware;
using AquaStore.Application;
using AquaStore.Infrastructure;
using AquaStore.Infrastructure.Data;
using Common.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add environment variables support
// .NET автоматически читает переменные окружения, но можно явно добавить
builder.Configuration.AddEnvironmentVariables();

// Add services to the container
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddSwagger();

// Add CORS
builder.Services.AddCorsPolicy(builder.Configuration);

// Add JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add Application layer
builder.Services.AddAquaStoreApplication();

// Add Infrastructure layer
builder.Services.AddAquaStoreInfrastructure(builder.Configuration);

// Email settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

var app = builder.Build();

// Apply database migrations automatically
try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AquaStoreDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("Applying database migrations...");
    dbContext.Database.Migrate();
    logger.LogInformation("Database migrations applied successfully.");
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while applying database migrations.");
    // Не останавливаем приложение, если миграции не применились
    // Это позволит приложению запуститься и показать более понятную ошибку
}

// Configure the HTTP request pipeline
// Swagger доступен в Development и можно включить в Production через переменную окружения
var enableSwagger = app.Environment.IsDevelopment() 
    || Environment.GetEnvironmentVariable("ENABLE_SWAGGER")?.ToLower() == "true";

if (enableSwagger)
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "swagger/{documentName}/swagger.json";
    });
    
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AquaStore API v1");
        c.RoutePrefix = string.Empty; // Swagger UI будет доступен по корневому пути /swagger
        c.DisplayRequestDuration();
        c.EnableTryItOutByDefault();
    });
}

// Global exception handling
app.UseExceptionHandling();

app.UseHttpsRedirection();

app.UseCors("DefaultPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
