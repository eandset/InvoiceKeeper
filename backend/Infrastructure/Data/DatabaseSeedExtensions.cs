using InvoiceKeeper.Infrastructure.Data.Seed;

namespace InvoiceKeeper.Infrastructure.Data;

public static class DatabaseSeedExtensions
{
    public static async Task SeedAdminAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var seeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();
        await seeder.SeedAsync();
    }
}

