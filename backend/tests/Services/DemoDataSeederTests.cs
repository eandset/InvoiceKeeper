using InvoiceKeeper.Domain.Enums;
using InvoiceKeeper.Infrastructure.Data;
using InvoiceKeeper.Infrastructure.Data.Seed;
using InvoiceKeeper.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InvoiceKeeper.Tests.Services;

public class DemoDataSeederTests
{
    [PostgresFact]
    public async Task SeedAsync_CreatesDemoPayments_WhenDatabaseIsEmpty()
    {
        await using var db = await PostgresTestDatabase.CreateAsync();
        var seeder = new DemoDataSeeder(db.Context, LoggerFactory.Create(_ => { }).CreateLogger<DemoDataSeeder>());

        await seeder.SeedAsync(CancellationToken.None);

        var payments = await db.Context.RecurringPayments
            .Include(p => p.Occurrences)
            .ToListAsync(CancellationToken.None);

        Assert.Equal(3, payments.Count);
        Assert.Contains(payments, p => p.Title == "Аренда офиса" && p.Currency == CurrencyType.RUB);
        Assert.Contains(payments, p => p.Title == "Сервер" && p.Currency == CurrencyType.USD);
        Assert.Contains(payments, p => p.Title == "Подписка" && p.Currency == CurrencyType.EUR);

        Assert.All(payments, p => Assert.Single(p.Occurrences));
    }

    [PostgresFact]
    public async Task SeedAsync_DoesNotDuplicatePayments_WhenRunTwice()
    {
        await using var db = await PostgresTestDatabase.CreateAsync();
        var seeder = new DemoDataSeeder(db.Context, LoggerFactory.Create(_ => { }).CreateLogger<DemoDataSeeder>());

        await seeder.SeedAsync(CancellationToken.None);
        await seeder.SeedAsync(CancellationToken.None);

        var count = await db.Context.RecurringPayments.CountAsync(CancellationToken.None);
        Assert.Equal(3, count);
    }
}
