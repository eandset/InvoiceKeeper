using InvoiceKeeper.Domain.Entities;
using InvoiceKeeper.Domain.Enums;
using InvoiceKeeper.Application.Common;
using InvoiceKeeper.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InvoiceKeeper.Application.Services;

/// <summary>Сервис для фонового пересчёта статуса просроченных периодов.</summary>
public sealed class OverdueRecalculationService
{
    private readonly AppDbContext _dbContext;
    private readonly IAppClock _clock;

    public OverdueRecalculationService(AppDbContext dbContext, IAppClock? clock = null)
    {
        _dbContext = dbContext;
        _clock = clock ?? new AppClock();
    }

    /// <summary>
    /// Помечает как просроченные все неоплаченные периоды с DueDate меньше сегодняшнего дня.
    /// </summary>
    public async Task<int> RecalculateAsync(CancellationToken cancellationToken = default)
    {
        var today = _clock.Today;

        var overdueOccurrences = await _dbContext.PaymentOccurrences
            .Where(o => o.Status == PaymentOccurrenceStatus.Unpaid &&
                        o.DueDate < today &&
                        o.RecurringPayment != null &&
                        !o.RecurringPayment.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var occurrence in overdueOccurrences)
        {
            occurrence.Status = PaymentOccurrenceStatus.Overdue;
            occurrence.UpdatedAt = _clock.UtcNow;
        }

        if (overdueOccurrences.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return overdueOccurrences.Count;
    }
}
