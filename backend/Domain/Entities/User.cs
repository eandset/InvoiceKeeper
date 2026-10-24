using InvoiceKeeper.Domain.Enums;

namespace InvoiceKeeper.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? TelegramId { get; set; }
    public long? TelegramChatId { get; set; }
}
