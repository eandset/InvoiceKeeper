using InvoiceKeeper.Application.DTOs;
using InvoiceKeeper.Application.Common;
using InvoiceKeeper.Domain.Entities;
using InvoiceKeeper.Domain.Enums;

namespace InvoiceKeeper.Application.Services;

internal class PaymentMapper
{
    public static PaymentOccurrenceResponseDto ToOccurrenceDto(
        PaymentOccurrence occurrence,
        RecurringPayment payment,
        DateTime today)
    {
        var daysUntilDue = (occurrence.DueDate.Date - today.Date).Days;
        return new PaymentOccurrenceResponseDto(
            occurrence.Id,
            occurrence.RecurringPaymentId,
            payment.Title,
            payment.Amount,
            payment.Currency.ToString(),
            occurrence.DueDate,
            occurrence.Status.ToString(),
            occurrence.ActualPaymentDate,
            occurrence.Status == PaymentOccurrenceStatus.Overdue,
            daysUntilDue);
    }

    public static PaymentResponseDto ToPaymentDto(RecurringPayment payment, DateTime today)
    {
        var nextOccurrence = payment.Occurrences
            .Where(o => o.Status != PaymentOccurrenceStatus.Paid)
            .OrderBy(o => o.DueDate)
            .FirstOrDefault();

        return new PaymentResponseDto(
            payment.Id,
            payment.Title,
            payment.Amount,
            payment.Currency.ToString(),
            payment.Periodicity,
            payment.NextPaymentDate,
            payment.IsDeleted,
            payment.CreatedAt,
            nextOccurrence is null ? null : ToOccurrenceDto(nextOccurrence, payment, today),
            payment.Occurrences.Count(o => o.Status == PaymentOccurrenceStatus.Paid),
            payment.Occurrences.Count(o => o.Status != PaymentOccurrenceStatus.Paid));
    }
}
