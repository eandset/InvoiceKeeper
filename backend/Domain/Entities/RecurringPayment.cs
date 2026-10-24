using InvoiceKeeper.Domain.Enums;

namespace InvoiceKeeper.Domain.Entities;

public class RecurringPayment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public CurrencyType Currency { get; set; } = CurrencyType.RUB;
    public string Periodicity { get; set; } = "monthly";
    public DateTime NextPaymentDate { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Навигационное свойство
    public ICollection<PaymentOccurrence> Occurrences { get; set; } = new List<PaymentOccurrence>();
}
