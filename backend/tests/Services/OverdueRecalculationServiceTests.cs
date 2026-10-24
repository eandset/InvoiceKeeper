using InvoiceKeeper.Application.DTOs;
using InvoiceKeeper.Application.Services;
using InvoiceKeeper.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace InvoiceKeeper.Tests.Services;

public class OverdueRecalculationServiceTests
{
    [PostgresFact]
    public async Task RecalculateAsync_MarksUnpaidOccurrencesAsOverdue_WhenDueDateIsInPast()
    {
        await using var db = await PostgresTestDatabase.CreateAsync();
        var paymentService = new PaymentService(db.Context);
        var overdueService = new OverdueRecalculationService(db.Context);

        await paymentService.CreateAsync(
            new CreatePaymentDto("Просроченный", 100m, "RUB", DateTime.UtcNow.Date.AddDays(-5)),
            CancellationToken.None);

        var updated = await overdueService.RecalculateAsync(CancellationToken.None);

        Assert.Equal(1, updated);

        var occurrence = await db.Context.PaymentOccurrences.SingleAsync();
        Assert.Equal("Overdue", occurrence.Status.ToString());
    }

    [PostgresFact]
    public async Task RecalculateAsync_DoesNotMarkPaidOccurrences()
    {
        await using var db = await PostgresTestDatabase.CreateAsync();
        var paymentService = new PaymentService(db.Context);
        var overdueService = new OverdueRecalculationService(db.Context);

        var payment = await paymentService.CreateAsync(
            new CreatePaymentDto("Оплаченный", 100m, "RUB", DateTime.UtcNow.Date.AddDays(-5)),
            CancellationToken.None);

        await paymentService.UpdateStatusAsync(
            payment.NextOccurrence!.Id,
            new UpdatePaymentStatusDto(true, null),
            CancellationToken.None);

        var updated = await overdueService.RecalculateAsync(CancellationToken.None);

        Assert.Equal(0, updated);
    }

    [PostgresFact]
    public async Task RecalculateAsync_DoesNotMarkFutureOccurrences()
    {
        await using var db = await PostgresTestDatabase.CreateAsync();
        var paymentService = new PaymentService(db.Context);
        var overdueService = new OverdueRecalculationService(db.Context);

        await paymentService.CreateAsync(
            new CreatePaymentDto("Будущий", 100m, "RUB", DateTime.UtcNow.Date.AddDays(5)),
            CancellationToken.None);

        var updated = await overdueService.RecalculateAsync(CancellationToken.None);

        Assert.Equal(0, updated);
    }
}
