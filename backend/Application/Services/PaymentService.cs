using InvoiceKeeper.Application.Common;
using InvoiceKeeper.Application.DTOs;
using InvoiceKeeper.Domain.Entities;
using InvoiceKeeper.Domain.Enums;
using InvoiceKeeper.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InvoiceKeeper.Application.Services;

public sealed class PaymentService
{
    private readonly AppDbContext _dbContext;

    public PaymentService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Активные (не удалённые) платежи вместе с ближайшим неоплаченным периодом.</summary>
    public async Task<IReadOnlyList<PaymentResponseDto>> GetActivePaymentsAsync(CancellationToken cancellationToken)
    {
        var payments = await _dbContext.RecurringPayments
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .Include(p => p.Occurrences)
            .OrderBy(p => p.Title)
            .ToListAsync(cancellationToken);

        return payments.Select(PaymentMapper.ToPaymentDto).ToList();
    }

    public async Task<PaymentResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var payment = await _dbContext.RecurringPayments
            .AsNoTracking()
            .Include(p => p.Occurrences)
            .SingleOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken)
            ?? throw new NotFoundException($"Платёж {id} не найден.");

        return PaymentMapper.ToPaymentDto(payment);
    }

    public async Task<PaymentResponseDto> CreateAsync(
        CreatePaymentDto request,
        CancellationToken cancellationToken)
    {
        var title = request.Title.Trim();

        if (!TryParseCurrency(request.Currency, out var currency))
        {
            throw new BadRequestException("Валюта должна быть одной из: RUB, USD, EUR.");
        }

        var periodicity = PeriodicityCalculator.Normalize(request.Periodicity);
        if (periodicity is null)
        {
            throw new BadRequestException(
                $"Периодичность должна быть одной из: {string.Join(", ", PeriodicityCalculator.Supported)}.");
        }

        var nextPaymentDate = request.NextPaymentDate.Date;
        var payment = new RecurringPayment
        {
            Title = title,
            Amount = decimal.Round(request.Amount, 2, MidpointRounding.AwayFromZero),
            Currency = currency,
            Periodicity = periodicity,
            NextPaymentDate = nextPaymentDate
        };

        payment.Occurrences.Add(new PaymentOccurrence
        {
            DueDate = nextPaymentDate
        });

        _dbContext.RecurringPayments.Add(payment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return PaymentMapper.ToPaymentDto(payment);
    }

    /// <summary>Мягкое удаление: платеж и его история остаются в БД.</summary>
    public async Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var payment = await _dbContext.RecurringPayments
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Платёж {id} не найден.");

        if (payment.IsDeleted)
        {
            return;
        }

        payment.IsDeleted = true;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Отмечает период оплаченным. После оплаты последнего неоплаченного периода
    /// автоматически создаётся следующий период согласно периодичности платежа.
    /// </summary>
    public async Task<PaymentOccurrenceResponseDto> UpdateStatusAsync(
        Guid occurrenceId,
        UpdatePaymentStatusDto request,
        CancellationToken cancellationToken)
    {
        var occurrence = await _dbContext.PaymentOccurrences
            .Include(o => o.RecurringPayment)
            .SingleOrDefaultAsync(o => o.Id == occurrenceId, cancellationToken)
            ?? throw new NotFoundException($"Период оплаты {occurrenceId} не найден.");

        var payment = occurrence.RecurringPayment
            ?? throw new ConflictException("Период оплаты не связан с регулярным платежом.");

        if (payment.IsDeleted)
        {
            throw new ConflictException("Платёж удалён, изменение его периодов недоступно.");
        }

        if (!request.IsPaid)
        {
            occurrence.IsPaid = false;
            occurrence.ActualPaymentDate = null;
            occurrence.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return PaymentMapper.ToOccurrenceDto(occurrence, payment);
        }

        occurrence.IsPaid = true;
        occurrence.ActualPaymentDate = (request.ActualPaymentDate ?? DateTime.UtcNow).Date;
        occurrence.ReminderSent = true;
        occurrence.UpdatedAt = DateTime.UtcNow;

        var hasLaterUnpaid = await _dbContext.PaymentOccurrences
            .AnyAsync(o => o.RecurringPaymentId == payment.Id &&
                           o.Id != occurrence.Id &&
                           !o.IsPaid &&
                           o.DueDate > occurrence.DueDate,
                cancellationToken);

        if (!hasLaterUnpaid)
        {
            var nextDueDate = PeriodicityCalculator.Next(occurrence.DueDate, payment.Periodicity);

            var exists = await _dbContext.PaymentOccurrences
                .AnyAsync(o => o.RecurringPaymentId == payment.Id && o.DueDate == nextDueDate,
                    cancellationToken);

            if (!exists)
            {
                _dbContext.PaymentOccurrences.Add(new PaymentOccurrence
                {
                    RecurringPaymentId = payment.Id,
                    DueDate = nextDueDate
                });
            }

            if (payment.NextPaymentDate < nextDueDate)
            {
                payment.NextPaymentDate = nextDueDate;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Возвращаем актуальное состояние периода после возможного добавления следующего.
        return PaymentMapper.ToOccurrenceDto(occurrence, payment);
    }

    /// <summary>Периоды, по которым нужно отправить напоминание: не оплачены, без напоминания, срок ≤ 3 дней.</summary>
    public async Task<IReadOnlyList<ReminderOccurrenceDto>> GetRemindersAsync(
        int withinDays,
        CancellationToken cancellationToken)
    {
        withinDays = Math.Clamp(withinDays, 0, 30);
        var today = DateTime.UtcNow.Date;
        var threshold = today.AddDays(withinDays);

        var occurrences = await _dbContext.PaymentOccurrences
            .AsNoTracking()
            .Where(o => !o.IsPaid &&
                        !o.ReminderSent &&
                        o.DueDate <= threshold &&
                        o.RecurringPayment != null &&
                        !o.RecurringPayment.IsDeleted)
            .Include(o => o.RecurringPayment)
            .OrderBy(o => o.DueDate)
            .ToListAsync(cancellationToken);

        return occurrences
            .Select(o => new ReminderOccurrenceDto(
                o.Id,
                o.RecurringPaymentId,
                o.RecurringPayment!.Title,
                o.RecurringPayment.Amount,
                o.RecurringPayment.Currency.ToString(),
                o.DueDate,
                (o.DueDate.Date - today).Days,
                !o.IsPaid && today > o.DueDate))
            .ToList();
    }

    public async Task MarkReminderSentAsync(Guid occurrenceId, CancellationToken cancellationToken)
    {
        var occurrence = await _dbContext.PaymentOccurrences
            .SingleOrDefaultAsync(o => o.Id == occurrenceId, cancellationToken)
            ?? throw new NotFoundException($"Период оплаты {occurrenceId} не найден.");

        if (occurrence.ReminderSent)
        {
            return;
        }

        occurrence.ReminderSent = true;
        occurrence.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Суммы предстоящих (неоплаченных) счетов по валютам и счётчики периодов.</summary>
    public async Task<PaymentSummaryDto> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;

        var unpaid = await _dbContext.PaymentOccurrences
            .AsNoTracking()
            .Where(o => !o.IsPaid && o.RecurringPayment != null && !o.RecurringPayment.IsDeleted)
            .Select(o => new
            {
                o.RecurringPayment!.Currency,
                o.RecurringPayment.Amount,
                o.DueDate
            })
            .ToListAsync(cancellationToken);

        return new PaymentSummaryDto(
            TotalRub: unpaid.Where(o => o.Currency == CurrencyType.RUB).Sum(o => o.Amount),
            TotalUsd: unpaid.Where(o => o.Currency == CurrencyType.USD).Sum(o => o.Amount),
            TotalEur: unpaid.Where(o => o.Currency == CurrencyType.EUR).Sum(o => o.Amount),
            PendingCount: unpaid.Count,
            OverdueCount: unpaid.Count(o => o.DueDate < today));
    }

    private static bool TryParseCurrency(string? value, out CurrencyType currency)
    {
        if (Enum.TryParse(value?.Trim(), ignoreCase: true, out currency))
        {
            return Enum.IsDefined(currency);
        }

        currency = default;
        return false;
    }
}

