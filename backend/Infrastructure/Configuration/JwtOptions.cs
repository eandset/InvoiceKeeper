namespace InvoiceKeeper.Infrastructure.Configuration;

/// <summary>Настройки выпуска и проверки JWT-токенов.</summary>
public sealed class JwtOptions
{
    public const string SectionName = "JWT";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "InvoiceKeeperBackend";
    public string Audience { get; set; } = "InvoiceKeeperClients";
    public int ExpiryInHours { get; set; } = 24;
}

