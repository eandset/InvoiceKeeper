using InvoiceKeeper.Domain.Entities;
using InvoiceKeeper.Domain.Enums;
using InvoiceKeeper.Infrastructure.Data;
using InvoiceKeeper.Infrastructure.Security;
using InvoiceKeeper.Tests.Support;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceKeeper.Tests.Api;

/// <summary>
/// Поднимает реальный API поверх временной базы PostgreSQL.
/// Параметры передаются через переменные окружения, которые читает Program.cs.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string AdminUsername = "api-admin";
    public const string AdminPassword = "Api-Admin-Passw0rd!";
    public const string UserUsername = "api-user";
    public const string UserPassword = "Api-User-Passw0rd!";
    public const string BotToken = "test-bot-token-for-integration-tests";
    public const string JwtSecret = "integration-tests-jwt-secret-0123456789abcdef";

    private string _databaseName = string.Empty;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            // Принудительно подключаем переменные окружения, чтобы секреты попадали в IConfiguration тестового хоста.
            var env = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            config.AddConfiguration(env);
        });
    }

    public async Task InitializeAsync()
    {
        var (databaseName, connectionString) = await PostgresTestServer.CreateDatabaseAsync();
        _databaseName = databaseName;

        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", connectionString);
        Environment.SetEnvironmentVariable("JWT_SECRET", JwtSecret);
        Environment.SetEnvironmentVariable("BOT_SERVICE_TOKEN", BotToken);
        Environment.SetEnvironmentVariable("Database__MigrateOnStartup", "true");
        Environment.SetEnvironmentVariable("Database__SeedOnStartup", "false");
        Environment.SetEnvironmentVariable("HttpsRedirection__Enabled", "false");

        // Обращение к Services запускает хост и применяет миграции.
        _ = Services;

        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = new Pbkdf2PasswordHasher();

        dbContext.Users.AddRange(
            new User
            {
                Username = AdminUsername,
                PasswordHash = hasher.Hash(AdminPassword),
                Role = UserRole.Admin
            },
            new User
            {
                Username = UserUsername,
                PasswordHash = hasher.Hash(UserPassword),
                Role = UserRole.User
            });

        await dbContext.SaveChangesAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();

        if (!string.IsNullOrEmpty(_databaseName))
        {
            await PostgresTestServer.DropDatabaseAsync(_databaseName);
        }
    }
}
