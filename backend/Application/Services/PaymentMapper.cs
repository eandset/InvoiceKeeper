using InvoiceKeeper.Application.DTOs;
using InvoiceKeeper.Domain.Entities;

namespace InvoiceKeeper.Application.Services;

internal static class PaymentMapper
{
    public static PaymentOccurrenceResponseDto ToOccurrenceDto(
        PaymentOccurrence occurrence,
        RecurringPayment payment) =>
        new(
            occurrence.Id,
            occurrence.RecurringPaymentId,
            payment.Title,
            payment.Amount,
            payment.Currency.ToString(),
            occurrence.DueDate,
            occurrence.IsPaid,
            occurrence.ActualPaymentDate,
            occurrence.IsOverdue);

    public static PaymentResponseDto ToPaymentDto(RecurringPayment payment)
    {
        var nextOccurrence = payment.Occurrences
            .Where(o => !o.IsPaid)
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
            nextOccurrence is null ? null : ToOccurrenceDto(nextOccurrence, payment),
            payment.Occurrences.Count(o => o.IsPaid),
            payment.Occurrences.Count(o => !o.IsPaid));
    }
}

