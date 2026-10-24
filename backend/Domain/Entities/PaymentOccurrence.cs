using InvoiceKeeper.Domain.Enums;

namespace InvoiceKeeper.Domain.Entities;

public class PaymentOccurrence
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid RecurringPaymentId { get; set; }
    public RecurringPayment? RecurringPayment { get; set; }

    public DateTime DueDate { get; set; }
    public PaymentOccurrenceStatus Status { get; set; } = PaymentOccurrenceStatus.Unpaid;
    public DateTime? ActualPaymentDate { get; set; }
    public bool ReminderSent { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsPaid => Status == PaymentOccurrenceStatus.Paid;

    public bool IsOverdue => Status == PaymentOccurrenceStatus.Overdue;
}

