namespace InvoiceKeeper.Domain.Entities;

public class PaymentOccurrence
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid RecurringPaymentId { get; set; }
    public RecurringPayment? RecurringPayment { get; set; }

    public DateTime DueDate { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? ActualPaymentDate { get; set; }
    public bool ReminderSent { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Вычисляемое свойство (не отображается в БД)
    public bool IsOverdue => !IsPaid && DateTime.UtcNow.Date > DueDate.Date;
}
