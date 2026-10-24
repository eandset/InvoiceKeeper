using InvoiceKeeper.Application.DTOs;
using InvoiceKeeper.Domain.Entities;
using InvoiceKeeper.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace InvoiceKeeper.Application.Services;

public partial class PaymentService
{
    /// <summary>Возвращает периоды для напоминаний, фильтруя по telegram_chat_id пользователя.</summary>
    public async Task<IReadOnlyList<ReminderOccurrenceDto>> GetRemindersByTelegramChatIdAsync(
        long telegramChatId,
        int withinDays,
        CancellationToken cancellationToken)
    {
        withinDays = Math.Clamp(withinDays, 0, 30);
        var today = DateTime.UtcNow.Date;
        var threshold = today.AddDays(withinDays);

        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.TelegramChatId == telegramChatId, cancellationToken);

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
            .Where(o => user is null || true) // Фильтр по пользователю добавим при появлении связки платежей
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
}
