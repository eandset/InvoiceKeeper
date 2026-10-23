namespace InvoiceKeeper.Tests.Support;

/// <summary>
/// Fact, который выполняется только при наличии PostgreSQL.
/// Иначе тест помечается пропущенным, а не падает.
/// </summary>
public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (TestEnvironment.Get("POSTGRES_PASSWORD") is null)
        {
            Skip = "Задайте POSTGRES_PASSWORD в .env или в переменных окружения, " +
                   "чтобы запустить интеграционные тесты с PostgreSQL.";
        }
    }
}

