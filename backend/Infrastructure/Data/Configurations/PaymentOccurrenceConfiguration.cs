using InvoiceKeeper.Domain.Enums;
using InvoiceKeeper.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceKeeper.Infrastructure.Data.Configurations;

public class PaymentOccurrenceConfiguration : IEntityTypeConfiguration<PaymentOccurrence>
{
    public void Configure(EntityTypeBuilder<PaymentOccurrence> builder)
    {
        builder.ToTable("payment_occurrences");

        builder.HasKey(po => po.Id);
        builder.Property(po => po.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(po => po.RecurringPaymentId)
            .HasColumnName("recurring_payment_id")
            .IsRequired();

        builder.Property(po => po.DueDate)
            .HasColumnName("due_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(po => po.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasDefaultValue(PaymentOccurrenceStatus.Unpaid)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(po => po.ActualPaymentDate)
            .HasColumnName("actual_payment_date")
            .HasColumnType("date");

        builder.Property(po => po.ReminderSent)
            .HasColumnName("reminder_sent")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(po => po.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Ignore(po => po.IsPaid);
        builder.Ignore(po => po.IsOverdue);

        builder.HasIndex(po => po.DueDate).HasDatabaseName("idx_occurrences_due_date");
        builder.HasIndex(po => po.Status).HasDatabaseName("idx_occurrences_status");
        builder.HasIndex(po => po.ReminderSent)
            .HasDatabaseName("idx_occurrences_reminder_sent")
            .HasFilter("reminder_sent = FALSE");

        builder.HasIndex(po => new { po.RecurringPaymentId, po.DueDate })
            .IsUnique()
            .HasDatabaseName("ux_occurrences_payment_due_date");
    }
}
