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

    public async Task<IReadOnlyList<PaymentOccurrenceResponseDto>> GetAllOccurrencesAsync(
        CancellationToken cancellationToken)
    {
        return await _dbContext.PaymentOccurrences
            .AsNoTracking()
            .Include(o => o.RecurringPayment)
            .Where(o => o.RecurringPayment != null && !o.RecurringPayment.IsDeleted)
            .OrderBy(o => o.DueDate)
            .ThenBy(o => o.RecurringPayment!.Title)
            .Select(o => new PaymentOccurrenceResponseDto(
                o.Id,
                o.RecurringPaymentId,
                o.RecurringPayment!.Title,
                o.RecurringPayment.Amount,
                o.RecurringPayment.Currency.ToString(),
                o.DueDate,
                o.Status.ToString(),
                o.ActualPaymentDate,
                o.Status == PaymentOccurrenceStatus.Overdue))
            .ToListAsync(cancellationToken);
    }

    public async Task<PaymentOccurrenceResponseDto> GetOccurrenceByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var occurrence = await _dbContext.PaymentOccurrences
            .AsNoTracking()
            .Include(o => o.RecurringPayment)
            .Where(o => o.Id == id && o.RecurringPayment != null && !o.RecurringPayment.IsDeleted)
            .Select(o => new PaymentOccurrenceResponseDto(
                o.Id,
                o.RecurringPaymentId,
                o.RecurringPayment!.Title,
                o.RecurringPayment.Amount,
                o.RecurringPayment.Currency.ToString(),
                o.DueDate,
                o.Status.ToString(),
                o.ActualPaymentDate,
                o.Status == PaymentOccurrenceStatus.Overdue))
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Период оплаты {id} не найден.");

        return occurrence;
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
            DueDate = nextPaymentDate,
            UpdatedAt = DateTime.UtcNow
        });

        _dbContext.RecurringPayments.Add(payment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return PaymentMapper.ToPaymentDto(payment);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var payment = await _dbContext.RecurringPayments
            .Include(p => p.Occurrences)
            .SingleOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken)
            ?? throw new NotFoundException($"Платёж {id} не найден.");

        payment.IsDeleted = true;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PaymentOccurrenceResponseDto> UpdateStatusAsync(
        Guid id,
        UpdatePaymentStatusDto request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<PaymentOccurrenceStatus>(request.Status, ignoreCase: true, out var status) ||
            !Enum.IsDefined(status))
        {
            throw new BadRequestException("Статус должен быть одним из: Unpaid, Paid, Overdue.");
        }

        var occurrence = await _dbContext.PaymentOccurrences
            .Include(o => o.RecurringPayment)
            .ThenInclude(p => p!.Occurrences)
            .SingleOrDefaultAsync(o => o.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Период оплаты {id} не найден.");

        var payment = occurrence.RecurringPayment;
        if (payment is null || payment.IsDeleted)
        {
            throw new NotFoundException($"Период оплаты {id} не найден.");
        }

        var previousStatus = occurrence.Status;
        var today = DateTime.UtcNow.Date;

        if (previousStatus == status)
        {
            return PaymentMapper.ToOccurrenceDto(occurrence, payment);
        }

        occurrence.Status = status;

        if (status == PaymentOccurrenceStatus.Paid)
        {
            occurrence.ActualPaymentDate = request.ActualPaymentDate?.Date ?? today;

            if (previousStatus != PaymentOccurrenceStatus.Paid)
            {
                EnsureNextOccurrence(payment, occurrence);
            }
        }
        else
        {
            occurrence.ActualPaymentDate = null;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return PaymentMapper.ToOccurrenceDto(occurrence, payment);
    }

    private static void EnsureNextOccurrence(
        RecurringPayment payment,
        PaymentOccurrence currentOccurrence)
    {
        var nextDueDate = PeriodicityCalculator.Next(currentOccurrence.DueDate, payment.Periodicity);
        var exists = payment.Occurrences.Any(o => o.DueDate == nextDueDate);

        if (!exists)
        {
            payment.Occurrences.Add(new PaymentOccurrence
            {
                Id = Guid.Empty,
                DueDate = nextDueDate,
                UpdatedAt = DateTime.UtcNow
            });
        }

        if (payment.NextPaymentDate < nextDueDate)
        {
            payment.NextPaymentDate = nextDueDate;
        }
    }

    public async Task<IReadOnlyList<ReminderOccurrenceDto>> GetRemindersAsync(
        int withinDays,
        CancellationToken cancellationToken)
    {
        withinDays = Math.Clamp(withinDays, 0, 30);
        var today = DateTime.UtcNow.Date;
        var threshold = today.AddDays(withinDays);

        var occurrences = await _dbContext.PaymentOccurrences
            .AsNoTracking()
            .Where(o => o.Status != PaymentOccurrenceStatus.Paid &&
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
                o.Status == PaymentOccurrenceStatus.Overdue))
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

    public async Task<PaymentSummaryDto> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;

        var unpaid = await _dbContext.PaymentOccurrences
            .AsNoTracking()
            .Where(o => o.Status != PaymentOccurrenceStatus.Paid &&
                        o.RecurringPayment != null &&
                        !o.RecurringPayment.IsDeleted)
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
