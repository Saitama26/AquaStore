using AquaStore.Api.Extensions;
using AquaStore.Api.Middleware;
using AquaStore.Application;
using AquaStore.Domain.Enums;
using AquaStore.Domain.Users;
using AquaStore.Infrastructure;
using AquaStore.Infrastructure.Data;
using Common.Infrastructure.Authentication;
using Common.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

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

    var seedDefaultAdmin = string.Equals(
        Environment.GetEnvironmentVariable("SEED_DEFAULT_ADMIN"),
        "true",
        StringComparison.OrdinalIgnoreCase) || app.Environment.IsDevelopment();

    if (seedDefaultAdmin)
    {
        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "gamerrayx851@gmail.com";
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin123!";
        var adminFirstName = Environment.GetEnvironmentVariable("ADMIN_FIRST_NAME") ?? "Admin";
        var adminLastName = Environment.GetEnvironmentVariable("ADMIN_LAST_NAME") ?? "User";

        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var adminPasswordHash = passwordHasher.Hash(adminPassword);

        var adminUser = dbContext.Users.FirstOrDefault(u => u.Email.Value == adminEmail);

        if (adminUser is null)
        {
            adminUser = User.Create(
                adminEmail,
                adminPasswordHash,
                adminFirstName,
                adminLastName,
                role: UserRole.Admin);

            adminUser.ConfirmEmail();
            dbContext.Users.Add(adminUser);

            logger.LogInformation("Default admin user created for {AdminEmail}.", adminEmail);
        }
        else
        {
            adminUser.SetRole(UserRole.Admin);
            adminUser.Activate();

            if (!adminUser.EmailConfirmed)
            {
                adminUser.ConfirmEmail();
            }

            adminUser.ChangePassword(adminPasswordHash);

            logger.LogInformation("Default admin user updated for {AdminEmail}.", adminEmail);
        }

        dbContext.SaveChanges();
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogCritical(ex, "Failed to apply database migrations. Application startup is aborted.");
    throw;
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
