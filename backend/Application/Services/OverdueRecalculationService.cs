using InvoiceKeeper.Domain.Entities;
using InvoiceKeeper.Domain.Enums;
using InvoiceKeeper.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InvoiceKeeper.Application.Services;

/// <summary>Сервис для фонового пересчёта статуса просроченных периодов.</summary>
public sealed class OverdueRecalculationService
{
    private readonly AppDbContext _dbContext;

    public OverdueRecalculationService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Помечает как просроченные все неоплаченные периоды с DueDate меньше сегодняшнего дня.
    /// </summary>
    public async Task<int> RecalculateAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;

        var overdueOccurrences = await _dbContext.PaymentOccurrences
            .Where(o => o.Status == PaymentOccurrenceStatus.Unpaid &&
                        o.DueDate < today &&
                        o.RecurringPayment != null &&
                        !o.RecurringPayment.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var occurrence in overdueOccurrences)
        {
            occurrence.Status = PaymentOccurrenceStatus.Overdue;
            occurrence.UpdatedAt = DateTime.UtcNow;
        }

        if (overdueOccurrences.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return overdueOccurrences.Count;
    }
}
