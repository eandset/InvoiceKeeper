using InvoiceKeeper.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceKeeper.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", t =>
            t.HasCheckConstraint("ck_users_role", "role IN ('Admin', 'User')"));

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.HasIndex(u => u.Username).IsUnique();

        builder.Property(u => u.Username)
            .HasColumnName("username")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.Role)
            .HasColumnName("role")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(u => u.TelegramId)
            .IsUnique()
            .HasDatabaseName("ux_users_telegram_id");

        builder.Property(u => u.TelegramId)
            .HasColumnName("telegram_id");

        builder.HasIndex(u => u.TelegramChatId)
            .IsUnique()
            .HasDatabaseName("ux_users_telegram_chat_id");

        builder.Property(u => u.TelegramChatId)
            .HasColumnName("telegram_chat_id");
    }
}
