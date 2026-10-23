using Microsoft.EntityFrameworkCore;

namespace InvoiceKeeper.Infrastructure.Data;

public static class DatabaseMigrationExtensions
{
    private const int MaxAttempts = 10;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Applies pending EF Core migrations, retrying while the database is still starting.
    /// </summary>
    public static async Task MigrateDatabaseAsync(this WebApplication app)
    {
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DatabaseMigrationExtensions));

        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully.");
                return;
            }
            catch (Exception exception) when (attempt < MaxAttempts)
            {
                logger.LogWarning(
                    exception,
                    "Database migration attempt {Attempt}/{MaxAttempts} failed. Retrying in {DelaySeconds} seconds.",
                    attempt,
                    MaxAttempts,
                    RetryDelay.TotalSeconds);

                await Task.Delay(RetryDelay);
            }
        }
    }
}
