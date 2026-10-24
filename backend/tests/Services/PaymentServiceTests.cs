using InvoiceKeeper.Application.Common;
using InvoiceKeeper.Application.DTOs;
using InvoiceKeeper.Application.Services;
using InvoiceKeeper.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace InvoiceKeeper.Tests.Services;

/// <summary>Проверки бизнес-логики платежей на реальном PostgreSQL.</summary>
public class PaymentServiceTests
{
    private static readonly DateTime DueDate = new(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc);

    [PostgresFact]
    public async Task CreateAsync_AddsFirstOccurrenceWithNextPaymentDate()
    {
        await using var db = await PostgresTestDatabase.CreateAsync();
        var service = new PaymentService(db.Context);

        var created = await service.CreateAsync(
            new CreatePaymentDto("Аренда", 100m, "RUB", DueDate),
            CancellationToken.None);

        Assert.Equal("monthly", created.Periodicity);
        Assert.Equal(1, created.UnpaidOccurrencesCount);
        Assert.NotNull(created.NextOccurrence);
        Assert.Equal(DueDate, created.NextOccurrence!.DueDate);

        var stored = await db.Context.PaymentOccurrences.SingleAsync();
        Assert.Equal(DueDate, stored.DueDate);
        Assert.False(stored.Status.ToString() == "Paid");
    }

    [PostgresFact]
    public async Task UpdateStatusAsync_MarksPaidAndCreatesNextMonth()
    {
        await using var db = await PostgresTestDatabase.CreateAsync();
        var service = new PaymentService(db.Context);
        var payment = await service.CreateAsync(
            new CreatePaymentDto("Аренда", 500m, "USD", DueDate),
            CancellationToken.None);

        var paid = await service.UpdateStatusAsync(
            payment.NextOccurrence!.Id,
            new UpdatePaymentStatusDto(true, null),
            CancellationToken.None);

        Assert.True(paid.Status == "Paid");
        Assert.Equal(DateTime.UtcNow.Date, paid.ActualPaymentDate);

        var occurrences = await db.Context.PaymentOccurrences
            .OrderBy(o => o.DueDate)
            .ToListAsync();

        Assert.Equal(2, occurrences.Count);
        Assert.True(occurrences[0].Status.ToString() == "Paid");
        Assert.False(occurrences[1].Status.ToString() == "Paid");
        Assert.Equal(DueDate.AddMonths(1), occurrences[1].DueDate);

        var storedPayment = await db.Context.RecurringPayments.SingleAsync();
        Assert.Equal(DueDate.AddMonths(1), storedPayment.NextPaymentDate);
    }

    [PostgresFact]
    public async Task UpdateStatusAsync_IsIdempotentWhenPaidTwice()
    {
        await using var db = await PostgresTestDatabase.CreateAsync();
        var service = new PaymentService(db.Context);
        var payment = await service.CreateAsync(
            new CreatePaymentDto("Интернет", 25m, "EUR", DueDate),
            CancellationToken.None);
        var occurrenceId = payment.NextOccurrence!.Id;

        await service.UpdateStatusAsync(occurrenceId, new UpdatePaymentStatusDto(true, null), CancellationToken.None);
        await service.UpdateStatusAsync(occurrenceId, new UpdatePaymentStatusDto(true, null), CancellationToken.None);

        Assert.Equal(2, await db.Context.PaymentOccurrences.CountAsync());
    }

    [PostgresFact]
    public async Task UpdateStatusAsync_UnpayingDoesNotDeleteNextPeriod()
    {
        await using var db = await PostgresTestDatabase.CreateAsync();
        var service = new PaymentService(db.Context);
        var payment = await service.CreateAsync(
            new CreatePaymentDto("Связь", 10m, "RUB", DueDate),
            CancellationToken.None);
        var occurrenceId = payment.NextOccurrence!.Id;

        await service.UpdateStatusAsync(occurrenceId, new UpdatePaymentStatusDto(true, null), CancellationToken.None);
        var unpaid = await service.UpdateStatusAsync(
            occurrenceId,
            new UpdatePaymentStatusDto(false, null),
            CancellationToken.None);

        Assert.False(unpaid.Status == "Paid");
        Assert.Null(unpaid.ActualPaymentDate);
    }

    [PostgresFact]
    public async Task CreateAsync_RejectsUnsupportedCurrency()
    {
        await using var db = await PostgresTestDatabase.CreateAsync();
        var service = new PaymentService(db.Context);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.CreateAsync(new CreatePaymentDto("X", 1m, "GBP", DueDate), CancellationToken.None));
    }

    [PostgresFact]
    public async Task SoftDelete_HidesPaymentAndStopsReminders()
    {
        await using var db = await PostgresTestDatabase.CreateAsync();
        var service = new PaymentService(db.Context);
        var dueSoon = DateTime.UtcNow.Date.AddDays(1);
        var payment = await service.CreateAsync(
            new CreatePaymentDto("Удаляемый", 99m, "RUB", dueSoon),
            CancellationToken.None);

        Assert.Single(await service.GetActivePaymentsAsync(CancellationToken.None));
        Assert.Single(await service.GetRemindersAsync(3, CancellationToken.None));

        await service.SoftDeleteAsync(payment.Id, CancellationToken.None);

        Assert.Empty(await service.GetActivePaymentsAsync(CancellationToken.None));
        Assert.Empty(await service.GetRemindersAsync(3, CancellationToken.None));
        Assert.True((await db.Context.RecurringPayments.SingleAsync()).IsDeleted);
    }

    [PostgresFact]
    public async Task GetSummaryAsync_SumsUnpaidAmountsPerCurrency()
    {
        await using var db = await PostgresTestDatabase.CreateAsync();
        var service = new PaymentService(db.Context);
        var overdueDate = DateTime.UtcNow.Date.AddDays(-2);

        await service.CreateAsync(new CreatePaymentDto("A", 100m, "RUB", overdueDate), CancellationToken.None);
        await service.CreateAsync(new CreatePaymentDto("B", 200.25m, "USD", DateTime.UtcNow.Date.AddDays(5)), CancellationToken.None);
        await service.CreateAsync(new CreatePaymentDto("C", 300m, "EUR", DateTime.UtcNow.Date.AddDays(10)), CancellationToken.None);

        var summary = await service.GetSummaryAsync(CancellationToken.None);

        Assert.Equal(100m, summary.TotalRub);
        Assert.Equal(200.25m, summary.TotalUsd);
        Assert.Equal(300m, summary.TotalEur);
        Assert.Equal(3, summary.PendingCount);
        Assert.Equal(1, summary.OverdueCount);
    }
}






