namespace InvoiceKeeper.Infrastructure.Configuration;

/// <summary>Настройки аутентификации сервисных клиентов (Telegram-бот).</summary>
public sealed class BotAuthOptions
{
    public const string SectionName = "BotAuth";

    public string Token { get; set; } = string.Empty;
    public string ServiceName { get; set; } = "InvoiceKeeperTelegramBot";
}

