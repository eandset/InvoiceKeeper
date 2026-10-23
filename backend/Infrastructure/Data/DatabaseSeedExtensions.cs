using InvoiceKeeper.Infrastructure.Data.Seed;

namespace InvoiceKeeper.Infrastructure.Data;

public static class DatabaseSeedExtensions
{
    public static async Task SeedAdminAsync(this WebApplication app)
    {
        try
        {
            await using var scope = app.Services.CreateAsyncScope();
            var seeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();
            await seeder.SeedAsync();
        }
        catch (Exception exception)
        {
            // База может быть недоступна при локальном запуске: приложение всё равно должно стартовать.
            var logger = app.Services.GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(DatabaseSeedExtensions));
            logger.LogWarning(exception, "Не удалось инициализировать администратора при старте.");
        }
    }
}

