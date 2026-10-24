using InvoiceKeeper.Domain.Entities;
using InvoiceKeeper.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace InvoiceKeeper.Infrastructure.Data.Seed;

/// <summary>Seed-демо-данные: тестовые регулярные платежи.</summary>
public sealed class DemoDataSeeder
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<DemoDataSeeder> _logger;

    public DemoDataSeeder(AppDbContext dbContext, ILogger<DemoDataSeeder> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Создаёт демо-платежи, если их ещё нет.
    /// Платежи привязываются к конкретному дню в текущем месяце, чтобы сразу были видны в UI.
    /// </summary>
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _dbContext.RecurringPayments.AnyAsync(cancellationToken))
            {
                _logger.LogInformation("Демо-данные уже существуют.");
                return;
            }

            var today = DateTime.UtcNow.Date;
            var currentMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var demoPayments = new List<RecurringPayment>
            {
                CreatePayment("Аренда офиса", 150_000m, CurrencyType.RUB, currentMonthStart, "monthly", 5),
                CreatePayment("Сервер", 85.50m, CurrencyType.USD, currentMonthStart, "monthly", 10),
                CreatePayment("Подписка", 49.99m, CurrencyType.EUR, currentMonthStart, "monthly", 15)
            };

            _dbContext.RecurringPayments.AddRange(demoPayments);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Созданы демо-данные: {Count} платежа.", demoPayments.Count);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Не удалось создать демо-данные.");
            throw;
        }
    }

    private static RecurringPayment CreatePayment(
        string title,
        decimal amount,
        CurrencyType currency,
        DateTime monthStart,
        string periodicity,
        int dueDay)
    {
        var daysInMonth = DateTime.DaysInMonth(monthStart.Year, monthStart.Month);
        var safeDay = Math.Min(dueDay, daysInMonth);
        var dueDate = monthStart.AddDays(safeDay - 1);

        var payment = new RecurringPayment
        {
            Title = title,
            Amount = amount,
            Currency = currency,
            Periodicity = periodicity,
            NextPaymentDate = dueDate,
            Occurrences = new List<PaymentOccurrence>()
        };

        payment.Occurrences.Add(new PaymentOccurrence
        {
            DueDate = dueDate,
            UpdatedAt = DateTime.UtcNow
        });

        return payment;
    }
}
