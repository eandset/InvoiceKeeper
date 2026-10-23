using InvoiceKeeper.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceKeeper.Infrastructure.Data.Configurations;

public class RecurringPaymentConfiguration : IEntityTypeConfiguration<RecurringPayment>
{
    public void Configure(EntityTypeBuilder<RecurringPayment> builder)
    {
        builder.ToTable("recurring_payments", t =>
            t.HasCheckConstraint("ck_recurring_payments_currency", "currency IN ('RUB', 'USD', 'EUR')"));

        builder.HasKey(rp => rp.Id);
        builder.Property(rp => rp.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(rp => rp.Title)
            .HasColumnName("title")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(rp => rp.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(rp => rp.Currency)
            .HasColumnName("currency")
            .HasConversion<string>()
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(rp => rp.Periodicity)
            .HasColumnName("periodicity")
            .HasMaxLength(20)
            .HasDefaultValue("monthly")
            .IsRequired();

        builder.Property(rp => rp.NextPaymentDate)
            .HasColumnName("next_payment_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(rp => rp.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(rp => rp.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasMany(rp => rp.Occurrences)
            .WithOne(po => po.RecurringPayment)
            .HasForeignKey(po => po.RecurringPaymentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
