namespace InvoiceKeeper.Api.Configuration;

public static class CorsConfiguration
{
    public const string PolicyName = "WebClients";

    /// <summary>
    /// Настраивает CORS для веб-клиента. Список источников задаётся через
    /// <c>Cors:AllowedOrigins</c> (массив) или переменную <c>CORS_ALLOWED_ORIGINS</c>
    /// со значениями через запятую.
    /// </summary>
    public static IServiceCollection AddWebClientCors(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var configured = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        var raw = configuration["CORS_ALLOWED_ORIGINS"];

        var origins = configured is { Length: > 0 }
            ? configured
            : raw?.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
              ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                if (origins.Length == 0 && environment.IsDevelopment())
                {
                    // Локальная разработка: разрешаем любой источник только в Development.
                    policy.SetIsOriginAllowed(_ => true)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    return;
                }

                // Без явно заданных источников в остальных окружениях CORS фактически выключен:
                // ни один источник не проходит проверку.
                if (origins.Length > 0)
                {
                    policy.WithOrigins(origins);
                }

                policy.AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }
}

