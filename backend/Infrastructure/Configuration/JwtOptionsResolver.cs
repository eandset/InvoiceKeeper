using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace InvoiceKeeper.Infrastructure.Configuration;

public static class JwtOptionsResolver
{
    private const int MinimumSecretLength = 32;

    /// <summary>
    /// Читает настройки JWT из секции <c>JWT</c> и переменной окружения <c>JWT_SECRET</c>.
    /// В Development допускается небезопасный секрет по умолчанию, чтобы API запускалось локально без .env.
    /// </summary>
    public static JwtOptions Resolve(IConfiguration configuration, IHostEnvironment environment)
    {
        var options = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

        if (string.IsNullOrWhiteSpace(options.Secret))
        {
            options.Secret = configuration["JWT_SECRET"] ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(options.Secret) && environment.IsDevelopment())
        {
            options.Secret = "invoice-keeper-development-only-secret-key";
        }

        if (options.Secret.Length < MinimumSecretLength)
        {
            throw new InvalidOperationException(
                $"JWT-секрет не задан или короче {MinimumSecretLength} символов. " +
                "Укажите JWT_SECRET в .env или JWT:Secret в конфигурации.");
        }

        if (options.ExpiryInHours <= 0)
        {
            options.ExpiryInHours = 24;
        }

        return options;
    }
}

