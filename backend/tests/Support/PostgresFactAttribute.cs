namespace InvoiceKeeper.Tests.Support;

/// <summary>
/// Fact, который выполняется только при наличии PostgreSQL.
/// Иначе тест помечается пропущенным, а не падает.
/// </summary>
public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")))
        {
            Skip = "Задайте POSTGRES_PASSWORD, чтобы запустить интеграционные тесты с PostgreSQL.";
        }
    }
}

