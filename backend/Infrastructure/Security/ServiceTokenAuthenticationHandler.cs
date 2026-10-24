using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using InvoiceKeeper.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace InvoiceKeeper.Infrastructure.Security;

/// <summary>
/// Аутентификация сервисных клиентов по статическому токену в заголовке <c>X-Service-Token</c>.
/// Используется Telegram-ботом; в токен не попадает никаких пользовательских данных.
/// </summary>
public sealed class ServiceTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "ServiceToken";
    public const string HeaderName = "X-Service-Token";
    public const string ServiceRole = "Service";

    private readonly BotAuthOptions _botAuthOptions;

    public ServiceTokenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IOptions<BotAuthOptions> botAuthOptions)
        : base(options, logger, encoder)
    {
        _botAuthOptions = botAuthOptions.Value;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var providedHeader) ||
            providedHeader.Count == 0 ||
            string.IsNullOrWhiteSpace(providedHeader[0]))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (string.IsNullOrWhiteSpace(_botAuthOptions.Token))
        {
            return Task.FromResult(AuthenticateResult.Fail(
                "Аутентификация сервиса не настроена: задайте BotAuth:Token или BOT_SERVICE_TOKEN."));
        }

        var provided = Encoding.UTF8.GetBytes(providedHeader[0]!);
        var expected = Encoding.UTF8.GetBytes(_botAuthOptions.Token);

        if (provided.Length != expected.Length ||
            !CryptographicOperations.FixedTimeEquals(provided, expected))
        {
            return Task.FromResult(AuthenticateResult.Fail("Неверный сервисный токен."));
        }

        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "service"),
                new Claim(ClaimTypes.Name, _botAuthOptions.ServiceName),
                new Claim(ClaimTypes.Role, ServiceRole)
            },
            SchemeName);

        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

