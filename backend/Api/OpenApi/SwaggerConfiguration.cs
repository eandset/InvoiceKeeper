using InvoiceKeeper.Infrastructure.Security;
using Microsoft.OpenApi;

namespace InvoiceKeeper.Api.OpenApi;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Invoice Keeper API",
                Version = "v1",
                Description = "REST API для учёта регулярных платежей, периодов оплаты и аналитики."
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT-токен, полученный через POST /api/v1/auth/login."
            });

            options.AddSecurityDefinition("ServiceToken", new OpenApiSecurityScheme
            {
                Name = ServiceTokenAuthenticationHandler.HeaderName,
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Description = "Сервисный токен Telegram-бота (BotAuth:Token)."
            });

            foreach (var xmlFile in Directory.GetFiles(AppContext.BaseDirectory, "backend.xml", SearchOption.TopDirectoryOnly))
            {
                options.IncludeXmlComments(xmlFile, includeControllerXmlComments: true);
            }
        });

        return services;
    }

    public static WebApplication UseApiDocumentation(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Invoice Keeper API v1");
            options.DocumentTitle = "Invoice Keeper API";
        });

        return app;
    }
}

