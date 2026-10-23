namespace InvoiceKeeper.Infrastructure.Configuration;

/// <summary>
/// Собирает строку подключения из конфигурации. Явная строка подключения имеет приоритет,
/// иначе параметры берутся из appsettings (без пароля) и переменной окружения POSTGRES_PASSWORD.
/// </summary>
public static class DatabaseConnectionString
{
    public static string Resolve(IConfiguration configuration)
    {
        var explicitConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(explicitConnectionString))
        {
            return explicitConnectionString;
        }

        var password = configuration["POSTGRES_PASSWORD"];
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Пароль базы данных не задан. Скопируйте .env.example в .env и укажите POSTGRES_PASSWORD.");
        }

        return Build(configuration, password);
    }

    public static string Build(IConfiguration configuration, string password) =>
        string.Join(';',
            $"Host={configuration["Database:Host"] ?? "localhost"}",
            $"Port={configuration["Database:Port"] ?? "5432"}",
            $"Database={configuration["Database:Name"] ?? "invoice_keeper"}",
            $"Username={configuration["Database:Username"] ?? "postgres"}",
            $"Password={password}");
}
